using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SmartCourt.Common.Exceptions;
using SmartCourt.Interfaces.Providers;

namespace SmartCourt.Providers.ChatModel;

public class AlibabaChatModelProvider : IChatModelProvider
{
    private readonly HttpClient _httpClient;
    private readonly AlibabaChatModelOptions _options;
    private readonly ILogger<AlibabaChatModelProvider> _logger;

    public AlibabaChatModelProvider(
        HttpClient httpClient,
        IOptions<AlibabaChatModelOptions> options,
        ILogger<AlibabaChatModelProvider> logger)
    {
        _httpClient = httpClient;
        _options = options.Value;
        _logger = logger;

        // Allow up to 10 minutes for large PDF reviews + AI generation
        _httpClient.Timeout = TimeSpan.FromMinutes(10);

        if (string.IsNullOrWhiteSpace(_options.ApiKey))
        {
            _logger.LogWarning("Alibaba DashScope API Key is not configured for Chat Model.");
        }
        else
        {
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _options.ApiKey);
        }

        if (!string.IsNullOrWhiteSpace(_options.BaseUrl))
        {
            var baseUrl = _options.BaseUrl.TrimEnd('/') + "/";
            if (Uri.TryCreate(baseUrl, UriKind.Absolute, out var baseUri))
            {
                _httpClient.BaseAddress = baseUri;
            }
        }
    }

    public async Task<string> GenerateAsync(string systemPrompt, string userPrompt, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("AlibabaChatModelProvider.GenerateAsync called. Model={Model}, SystemPromptLength={SysLen}, UserPromptLength={UserLen}",
            _options.Model, systemPrompt.Length, userPrompt.Length);

        var requestBody = new
        {
            model = _options.Model,
            messages = new[]
            {
                new { role = "system", content = systemPrompt },
                new { role = "user", content = userPrompt }
            },
            stream = false,
            max_tokens = _options.MaxTokens > 0 ? _options.MaxTokens : 2000
        };

        HttpResponseMessage? response = null;
        string? lastErrorMessage = null;
        int maxRetries = 2;
        int delayMs = 1000;

        for (int i = 0; i < maxRetries; i++)
        {
            try
            {
                response = await _httpClient.PostAsJsonAsync("chat/completions", requestBody, cancellationToken);

                if (response.IsSuccessStatusCode)
                {
                    _logger.LogInformation("Alibaba DashScope API returned success on attempt {Attempt}.", i + 1);
                    break;
                }

                lastErrorMessage = await response.Content.ReadAsStringAsync(cancellationToken);
                _logger.LogWarning("Alibaba DashScope API returned HTTP {StatusCode} on attempt {Attempt}: {Error}",
                    response.StatusCode, i + 1, lastErrorMessage);

                if (response.StatusCode == System.Net.HttpStatusCode.TooManyRequests && i < maxRetries - 1)
                {
                    _logger.LogWarning("Rate limit hit. Retrying in {Delay}ms...", delayMs);
                    await Task.Delay(delayMs, cancellationToken);
                    delayMs *= 2;
                    continue;
                }

                throw new BusinessException("خدمة المساعد الذكي غير متاحة حالياً، يرجى إعادة المحاولة لاحقاً.");
            }
            catch (BusinessException)
            {
                throw;
            }
            catch (TaskCanceledException ex) when (!cancellationToken.IsCancellationRequested)
            {
                _logger.LogError(ex, "Alibaba DashScope API request timed out on attempt {Attempt}.", i + 1);
                throw new BusinessException("خدمة المساعد الذكي غير متاحة حالياً، يرجى إعادة المحاولة لاحقاً.", ex);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to connect to Alibaba DashScope API on attempt {Attempt}.", i + 1);
                throw new BusinessException("خدمة المساعد الذكي غير متاحة حالياً، يرجى إعادة المحاولة لاحقاً.", ex);
            }
        }

        if (response == null || !response.IsSuccessStatusCode)
        {
            throw new BusinessException("خدمة المساعد الذكي غير متاحة حالياً، يرجى إعادة المحاولة لاحقاً.");
        }

        var rawBody = await response.Content.ReadAsStringAsync(cancellationToken);
        _logger.LogDebug("Alibaba DashScope raw response body: {RawBody}", rawBody);

        if (string.IsNullOrWhiteSpace(rawBody))
        {
            throw new BusinessException("خدمة المساعد الذكي غير متاحة حالياً، يرجى إعادة المحاولة لاحقاً.");
        }

        try
        {
            using var responseData = JsonDocument.Parse(rawBody);

            if (responseData.RootElement.TryGetProperty("choices", out var choices) && choices.GetArrayLength() > 0)
            {
                var message = choices[0].GetProperty("message");
                if (message.TryGetProperty("content", out var contentProp))
                {
                    return contentProp.GetString() ?? string.Empty;
                }
            }

            return responseData.RootElement.ToString();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to parse Alibaba DashScope API response: {Response}", rawBody);
            throw new BusinessException("خدمة المساعد الذكي غير متاحة حالياً، يرجى إعادة المحاولة لاحقاً.", ex);
        }
    }
    private static string GenerateFallbackResponse(string systemPrompt, string userPrompt)
    {
        if (systemPrompt.Contains("specialization", StringComparison.OrdinalIgnoreCase))
        {
            return GenerateTailoredClassificationFallback(userPrompt);
        }

        if (systemPrompt.Contains("Strength", StringComparison.OrdinalIgnoreCase) || systemPrompt.Contains("Weakness", StringComparison.OrdinalIgnoreCase))
        {
            return GenerateTailoredReviewFallback(userPrompt);
        }

        return "تمت مراجعة وتحليل طلبك بنجاح من المساعد القانوني.";
    }

    private static string GenerateTailoredReviewFallback(string userPrompt)
    {
        string title = ExtractPromptValue(userPrompt, "Case Title:");
        string description = ExtractPromptValue(userPrompt, "Case Description:");
        string governorate = ExtractPromptValue(userPrompt, "Governorate:");
        string city = ExtractPromptValue(userPrompt, "City:");

        var points = new List<object>();
        var textToAnalyze = $"{title} {description}".ToLowerInvariant();

        // Domain Detection
        bool isLabor = textToAnalyze.Contains("عمل") || textToAnalyze.Contains("عمال") || textToAnalyze.Contains("موظف") || textToAnalyze.Contains("راتب") || textToAnalyze.Contains("أجر") || textToAnalyze.Contains("استقالة") || textToAnalyze.Contains("فصل");
        bool isRental = textToAnalyze.Contains("إيجار") || textToAnalyze.Contains("مؤجر") || textToAnalyze.Contains("مستأجر") || textToAnalyze.Contains("شقة") || textToAnalyze.Contains("عين") || textToAnalyze.Contains("طرد") || textToAnalyze.Contains("إخلاء");
        bool isCommercial = textToAnalyze.Contains("تجاري") || textToAnalyze.Contains("شيك") || textToAnalyze.Contains("كمبيالة") || textToAnalyze.Contains("إيصال أمانة") || textToAnalyze.Contains("شركة") || textToAnalyze.Contains("توريد") || textToAnalyze.Contains("سجل تجاري");
        bool isFamily = textToAnalyze.Contains("أسرة") || textToAnalyze.Contains("طلاق") || textToAnalyze.Contains("نفقة") || textToAnalyze.Contains("حضانة") || textToAnalyze.Contains("مؤخر") || textToAnalyze.Contains("مسكن");

        var caseSubject = !string.IsNullOrWhiteSpace(title) && !title.Equals("Not specified", StringComparison.OrdinalIgnoreCase)
            ? title
            : "موضوع الدعوى";

        // 1. STRENGTH (تفوق جانب الموكل وحجية الإثبات)
        if (isLabor)
        {
            points.Add(new
            {
                type = "Strength",
                description = $"قوة الموقف الإثباتي للموكل تتمثل في إثبات علاقة العمل والتبعية الفعلية بخصوص '{caseSubject}'؛ حيث يدعم القانون المصري (قانون العمل رقم 12 لسنة 2003) العامل بإمكانية إثبات علاقة العمل بكافة طرق الإثبات القانونية بما فيها المراسلات وشهادة الشهود."
            });
        }
        else if (isRental)
        {
            points.Add(new
            {
                type = "Strength",
                description = $"تتجلى قوة جانب الموكل في وجود سند عقد الإيجارات بخصوص '{caseSubject}'، مما يعطي الموكل أولوية الحماية القانونية وفقاً لأحكام القانون المدني المصري رقم 4 لسنة 1996 وتعديلاته."
            });
        }
        else if (isCommercial)
        {
            points.Add(new
            {
                type = "Strength",
                description = $"تكمن قوة موقف الموكل في الحيازة الفعلية للسندات التجارية/العقود المبرمة بخصوص '{caseSubject}'، مما يوفر استحقاقاً مستقلاً ومباشراً يتيح اتخاذ إجراءات التنفيذ أو السير في الدعوى التجارية بحجية كاملة."
            });
        }
        else if (isFamily)
        {
            points.Add(new
            {
                type = "Strength",
                description = $"تتمثل قوة الموقف القانوني في وجود وثائق رسمية ثابتة التاريخ بخصوص '{caseSubject}'، مما يكفل للموكل الاستناد إلى قواعد الآمر الشرعي والقانوني المباشر أمام محكمة الأسرة."
            });
        }
        else
        {
            points.Add(new
            {
                type = "Strength",
                description = $"تتمثل نقطة القوة الأساسية في صياغة الموضوع بوضوح حول '{caseSubject}'، وتوافر السند المبدئي الذي يرجح كفة الموكل في إثبات أصل الالتزام وتفوقه إثباتياً على الخصم."
            });
        }

        // 2. WEAKNESS (مكاسب أو ميزات الطرف الآخر في النزاع)
        if (isLabor)
        {
            points.Add(new
            {
                type = "Weakness",
                description = "ميزة الطرف الآخر تتمثل في احتمال التمسك بعدم اللجوء لمكتب العمل خلال الميعاد القانوني (الـ 10 أيام وفقاً للمادة 70 من قانون العمل المصري)، أو الدفع بعدم ثبوت عناصر الأجر المتغير والبدلات بمستندات رسمية."
            });
        }
        else if (isRental)
        {
            points.Add(new
            {
                type = "Weakness",
                description = "قد يستغل الطرف الآخر عدم وجود إنذار رسمي صريح بالتكليف بالوفاء محرر عبر قلم المحضرين قبل التوجه للقضاء، مما يعطيه ثغرة إجرائية للدفع بعدم قبول الدعوى."
            });
        }
        else if (isCommercial)
        {
            points.Add(new
            {
                type = "Weakness",
                description = "الطرف الآخر قد يتفوق قانونياً في حال الدفع بصورية الدين، أو التمسك بخلو السندات من تواريخ استحقاق صريحة، أو عدم إثبات الرفض البنكي الرسمي (الريفوزو) للشيكات."
            });
        }
        else
        {
            points.Add(new
            {
                type = "Weakness",
                description = "ميزة الخصم تتمثل في غياب التوثيق الرسمي للتنبيهات أو الإخطارات المتبادلة بين الأطراف، مما يتيح له إنكار الاستلام أو الدفع بالتراخي في المطالبة."
            });
        }

        // 3. MISSING CASE INFO (المعلومات والنقاط المفقودة)
        var missingInfoList = new List<string>();
        if (string.IsNullOrWhiteSpace(governorate) || governorate.Equals("Not specified", StringComparison.OrdinalIgnoreCase))
        {
            missingInfoList.Add("تحديد المحافظة والمدينة بدقة لتحديد الاختصاص المحلي للمحكمة الجزئية/الابتدائية");
        }
        if (!description.Contains("جنيه", StringComparison.OrdinalIgnoreCase) && !description.Contains("مبلغ", StringComparison.OrdinalIgnoreCase) && !description.Contains("قيمة", StringComparison.OrdinalIgnoreCase))
        {
            missingInfoList.Add("حصر وتفصيص المبالغ المالية المطلوبة والتعويضات الدقيقة عن الضرر المادي والمعنوي");
        }
        missingInfoList.Add("إدراج التواريخ الرسمية الدقيقة لبدء النزاع وتاريخ الإخلال بالتعهدات");

        points.Add(new
        {
            type = "MissingCaseInfo",
            description = $"يحتاج الملف إلى استيفاء النقاط والمعلومات التالية لضمان صياغة صحيفة الدعوى بشكل مكتمل: {string.Join("، و", missingInfoList)}."
        });

        // 4. MISSING CASE DOCS (المستندات والوثائق المفقودة المحددة)
        if (isLabor)
        {
            points.Add(new
            {
                type = "MissingCaseDoc",
                description = "المستندات المحددة المطلوبة لإكمال الملف: أصل عقد العمل إن وجد، كشف حساب بنكي يثبت تحويلات الأجر، برينت التأمينات الاجتماعية (كشف الحساب التأميني)، صورة بطاقة الرقم القومي سارية، وشكوى مكتب العمل."
            });
        }
        else if (isRental)
        {
            points.Add(new
            {
                type = "MissingCaseDoc",
                description = "المستندات المحددة المطلوبة لإكمال الملف: أصل عقد الإيجار، صورة بطاقة الرقم القومي سارية، إيصالات سداد الأجرة السابقة، وصورة الإنذار الرسمي بالتكليف بالوفاء المعلن عبر قلم المحضرين."
            });
        }
        else if (isCommercial)
        {
            points.Add(new
            {
                type = "MissingCaseDoc",
                description = "المستندات المحددة المطلوبة لإكمال الملف: أصل الأوراق التجارية (شيكات / كمبيالات / إيصالات أمانة)، إفادة البنك بالرفض (الريفوزو)، أصل العقد أو فواتير التوريد الموقعة، وصورة بطاقة الرقم القومي أو السجل التجاري."
            });
        }
        else if (isFamily)
        {
            points.Add(new
            {
                type = "MissingCaseDoc",
                description = "المستندات المحددة المطلوبة لإكمال الملف: أصل وثيقة الزواج/الطلاق الرسمية، شهادات ميلاد الأبناء القصر مميكنة، كشف مفردات مرتب أو إثبات دخل الخصم، وصورة بطاقة الرقم القومي."
            });
        }
        else
        {
            points.Add(new
            {
                type = "MissingCaseDoc",
                description = "المستندات المحددة المطلوبة لإكمال الملف: أصل العقد/الاتفاق المبرم، صورة بطاقة الرقم القومي سارية لكل أطراف الدعوى، إيصالات التحويل أو السداد المالي، وأي إنذارات رسمية على يد محضر."
            });
        }

        // 5. SUGGESTIONS (مقترحات صياغة وهيكلة الدعوى - STRICTLY ZERO LAWYER MENTIONS)
        points.Add(new
        {
            type = "Suggestion",
            description = "قم بإعادة هيكلة وصف القضية في صورة جدول زمني متسلسل، يبدأ من تاريخ التعهد الأول، مروراً بتاريخ الإخلال، وصولاً إلى حجم الأضرار المترتبة حالياً."
        });

        points.Add(new
        {
            type = "Suggestion",
            description = "قم بتفقيط وقسمة كافة المطالبات المالية إلى بنود مستقلة (أصل الدين، الفوائد أو التعويض عن المماطلة، والرسوم) وتوثيق كل بند بسند كتابي مستقل."
        });

        points.Add(new
        {
            type = "Suggestion",
            description = "قم بتنظيم وثائق الملف في مجلد مرتب حسب التاريخ، وتأكد من مسح الأوراق ضوئياً بدقة عالية لضمان سهولة الإسناد والفحص القضائي."
        });

        return JsonSerializer.Serialize(points, new JsonSerializerOptions { WriteIndented = true });
    }

    private static string GenerateTailoredClassificationFallback(string userPrompt)
    {
        string title = ExtractPromptValue(userPrompt, "Case Title:").ToLowerInvariant();
        string desc = ExtractPromptValue(userPrompt, "Case Description:").ToLowerInvariant();
        string text = $"{title} {desc}";

        string spec = "CivilLaw";
        if (text.Contains("أسرة") || text.Contains("طلاق") || text.Contains("نفقة") || text.Contains("خلع") || text.Contains("حضانة") || text.Contains("تركة") || text.Contains("ميراث"))
        {
            spec = "FamilyLaw";
        }
        else if (text.Contains("عمل") || text.Contains("عمال") || text.Contains("راتب") || text.Contains("أجر") || text.Contains("استقالة") || text.Contains("فصل") || text.Contains("موظف") || text.Contains("وظيفة") || text.Contains("إجازات") || text.Contains("مكافأة"))
        {
            spec = "LaborLaw";
        }
        else if (text.Contains("جناية") || text.Contains("سرقة") || text.Contains("جنحة") || text.Contains("محضر") || text.Contains("نصب") || text.Contains("تزوير") || text.Contains("احتيال") || text.Contains("جريمة"))
        {
            spec = "CriminalLaw";
        }
        else if (text.Contains("مجلس الدولة") || text.Contains("قرار إداري") || text.Contains("حكومة") || text.Contains("مناقصة"))
        {
            spec = "AdministrativeAndStateCouncilLaw";
        }
        else if (text.Contains("تجاري") || text.Contains("شيك") || text.Contains("كمبيالة") || text.Contains("إيصال أمانة") || text.Contains("سجل تجاري") || text.Contains("شراكة") || text.Contains("مساهمين") || (text.Contains("شركة") && !text.Contains("عمل") && !text.Contains("راتب") && !text.Contains("فصل")))
        {
            spec = "CommercialLaw";
        }

        return JsonSerializer.Serialize(new
        {
            specialization = spec,
            requiredLawyerLevel = "PrimaryCourt",
            complexity = "Standard"
        });
    }

    private static string ExtractPromptValue(string text, string key)
    {
        var index = text.IndexOf(key, StringComparison.OrdinalIgnoreCase);
        if (index == -1) return string.Empty;

        var start = index + key.Length;
        var end = text.IndexOf('\n', start);
        var line = end == -1 ? text[start..] : text[start..end];
        return line.Trim();
    }
}
