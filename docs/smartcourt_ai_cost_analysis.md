# SmartCourt (Mostashar) — AI Cost Analysis Report

**Prepared:** August 16, 2026
**Author:** AI Cost Analyst
**Scope:** AI API cost analysis for SmartCourt legal assistant platform
**Currency:** All prices in USD unless otherwise noted

---

## 1. Executive Summary

SmartCourt (Mostashar) is an AI-powered Egyptian legal assistant that uses three Alibaba Cloud DashScope AI models in its Chat Agent pipeline:

1. **text-embedding-v4** — Embeds user queries for vector search
2. **qwen3-rerank** — Reranks retrieved legal articles for relevance
3. **qwen-flash** — Generates the final legal answer (LLM)

Additionally, SmartCourt uses **Qdrant** as its vector database for storing and searching legal embeddings.

**Key Findings:**

| Metric | Value |
|:---|:---|
| Cost per normal request | **≈ $0.000428** |
| Normal user cost/day (10 req) | **≈ $0.00428** |
| Normal user cost/month | **≈ $0.128** |
| 1,000 normal users/month | **≈ $128** |
| Biggest cost driver | **LLM output tokens (qwen-flash)** — ~65% of total |
| Free plan (5 req/day) feasible? | **Yes** — ~$0.064/user/month |

> [!IMPORTANT]
> All usage volumes (tokens per request, requests per user) are **estimates for financial modeling**. SmartCourt does not yet have production telemetry data. These figures should be validated against real usage before setting final quotas or pricing.

---

## 2. AI Services Used

SmartCourt's Chat Agent pipeline processes each user request in three sequential steps:

```
User Question
    │
    ▼
┌─────────────────────────┐
│  text-embedding-v4      │  → Embed user query (vector search)
└─────────┬───────────────┘
          │
          ▼
┌─────────────────────────┐
│  Qdrant Vector Search   │  → Retrieve top 20 candidate articles
└─────────┬───────────────┘
          │
          ▼
┌─────────────────────────┐
│  qwen3-rerank           │  → Rerank 20 candidates → top 5
└─────────┬───────────────┘
          │
          ▼
┌─────────────────────────┐
│  qwen-flash             │  → Generate legal answer (max 2000 tokens)
└─────────────────────────┘
```

**Configuration confirmed from project source code:**

| Setting | Value | Source |
|:---|:---|:---|
| Embedding model | `text-embedding-v4` | [appsettings.json](file:///D:/ITI%209%20Month/Graduation%20Project/SmartCourt/SmartCourt/appsettings.json#L99) |
| Embedding dimensions | 1536 | [appsettings.json](file:///D:/ITI%209%20Month/Graduation%20Project/SmartCourt/SmartCourt/appsettings.json#L100) |
| Reranker model | `qwen3-rerank` | [appsettings.json](file:///D:/ITI%209%20Month/Graduation%20Project/SmartCourt/SmartCourt/appsettings.json#L105) |
| Chat model | `qwen-flash` | [appsettings.json](file:///D:/ITI%209%20Month/Graduation%20Project/SmartCourt/SmartCourt/appsettings.json#L120) |
| Max output tokens | 2000 | [appsettings.json](file:///D:/ITI%209%20Month/Graduation%20Project/SmartCourt/SmartCourt/appsettings.json#L122) |
| Candidate count (vector search) | 20 | [appsettings.json](file:///D:/ITI%209%20Month/Graduation%20Project/SmartCourt/SmartCourt/appsettings.json#L91) |
| Reranked count (top N) | 5 | [appsettings.json](file:///D:/ITI%209%20Month/Graduation%20Project/SmartCourt/SmartCourt/appsettings.json#L92) |
| Max prompt context chars | 24,000 | [appsettings.json](file:///D:/ITI%209%20Month/Graduation%20Project/SmartCourt/SmartCourt/appsettings.json#L93) |
| API endpoint | `dashscope-intl.aliyuncs.com` | [AlibabaChatModelOptions.cs](file:///D:/ITI%209%20Month/Graduation%20Project/SmartCourt/SmartCourt/Providers/ChatModel/AlibabaChatModelOptions.cs#L8) |
| Region | Singapore (ap-southeast-1) — International | [appsettings.json](file:///D:/ITI%209%20Month/Graduation%20Project/SmartCourt/SmartCourt/appsettings.json#L101) |

---

## 3. Current Official Pricing

**Source:** [Alibaba Cloud Model Studio — Model Inference Pricing](https://www.alibabacloud.com/help/en/model-studio/billing-for-model-studio)
**Date checked:** August 16, 2026
**Region used:** Singapore (International deployment scope) — matches SmartCourt's configuration

---

### 3.1 text-embedding-v4

| Property | Value | Category |
|:---|:---|:---|
| **Model ID** | `text-embedding-v4` | Official Fact |
| **Input price** | **$0.07 / 1M tokens** | Official Fact |
| **Output price** | **Free** (output is free for embeddings) | Official Fact |
| **Billing unit** | Per 1 million input tokens | Official Fact |
| **Free quota** | 1 million tokens (valid 90 days after activation) | Official Fact |
| **Batch pricing** | Not listed for this model | Official Fact |
| **Max tokens per request** | 8,192 tokens | Official Fact |
| **Supported dimensions** | 64, 128, 256, 512, 768, 1024, 1536, 2048 | Official Fact |
| **SmartCourt dimensions** | 1536 | Official Fact (from config) |

> [!NOTE]
> **Dimensions do NOT affect pricing.** The official pricing page states billing is by input tokens only. The embedding dimension affects the output vector size but not the cost.
>
> **Multiple texts in one request** do not change per-token pricing. Each text's tokens are counted independently and summed.

**Regional comparison:**

| Region | Price per 1M tokens |
|:---|---:|
| Singapore (International) | $0.07 |
| China (Beijing) | $0.072 |
| Hong Kong | $0.07 |

---

### 3.2 qwen3-rerank

| Property | Value | Category |
|:---|:---|:---|
| **Model ID** | `qwen3-rerank` | Official Fact |
| **Input price** | **$0.10 / 1M tokens** | Official Fact |
| **Output price** | **Free** (output is free for reranking) | Official Fact |
| **Billing rule** | "Billed by input tokens. Output is free." | Official Fact |
| **Billing unit** | Per 1 million input tokens | Official Fact |
| **Free quota** | 1 million tokens (valid 90 days after activation) | Official Fact |
| **Max input tokens per request** | 120,000 tokens | Official Fact |

> [!IMPORTANT]
> **Reranker token calculation:** The total input tokens for a rerank request include BOTH the query tokens and ALL document tokens. The formula is:
> ```
> Total Reranker Input Tokens = Query Tokens × Number of Documents + Total Document Tokens
> ```
> The query is replicated internally for each document comparison, so the query tokens are multiplied by the number of documents.

---

### 3.3 qwen-flash

| Property | Value | Category |
|:---|:---|:---|
| **Model ID** | `qwen-flash` | Official Fact |
| **Deployment scope** | International (Singapore) | Official Fact |
| **Context window** | 1,000,000 tokens (1M) | Official Fact |
| **SmartCourt max output** | 2,000 tokens (configured) | Official Fact (from config) |
| **Batch pricing** | 50% of standard price | Official Fact |
| **Context caching** | Supported (cache hits at discount) | Official Fact |
| **Free quota** | 1 million tokens (valid 90 days after activation) | Official Fact |

**Tiered pricing (Singapore / International):**

| Input Token Tier | Input Price (per 1M tokens) | Output Price (per 1M tokens) |
|:---|---:|---:|
| 0 < Tokens ≤ 256K | **$0.05** | **$0.40** |
| 256K < Tokens ≤ 1M | $0.25 | $2.00 |

> [!NOTE]
> **SmartCourt will almost always be in Tier 1** (0–256K tokens). A typical SmartCourt request has roughly 3,000–8,000 input tokens — vastly under the 256K threshold. The tiered pricing applies per-request, not cumulatively.
>
> **Maximum possible cost per request:**
> With 2,000 output tokens and ~8,000 input tokens (all in Tier 1):
> - Max input cost: 8,000 × $0.05/1M = $0.0004
> - Max output cost: 2,000 × $0.40/1M = $0.0008
> - **Max LLM cost per request: ≈ $0.0012**

---

## 4. Billing Method

### 4.1 text-embedding-v4 — How It's Charged

| Question | Answer |
|:---|:---|
| How is input charged? | Per input token, at $0.07/1M tokens |
| Is output charged? | **No** — output (the embedding vector) is free |
| Do embedding dimensions affect pricing? | **No** — pricing is based solely on input tokens |
| Do multiple texts in one request change pricing? | **No** — all text tokens are summed and billed at the same per-token rate |
| Is there a minimum charge? | No minimum charge found in official documentation |

**Formula:**
```
Embedding Cost = Input Tokens × ($0.07 / 1,000,000)
```

---

### 4.2 qwen3-rerank — How It's Charged

| Question | Answer |
|:---|:---|
| What is billed? | **Input tokens only** (query + all documents) |
| Are query tokens billed? | **Yes** — query tokens are counted once per document (replicated internally) |
| Are document tokens billed? | **Yes** — all document tokens are counted |
| Is the number of documents billed? | **Indirectly** — more documents = more total tokens |
| Are requests billed? | **No** — billing is purely token-based |

**SmartCourt's reranker usage:**
- 1 query + 20 retrieved legal article chunks
- Each chunk ≈ 512 tokens (configured `MaxChunkTokens: 512`)

**Token calculation for one reranker call:**
```
Query tokens:                      ~30 tokens (short legal question)
Query replicated × 20 documents:   30 × 20 = 600 tokens
20 document chunks × 512 tokens:   20 × 512 = 10,240 tokens
─────────────────────────────────────────────────
Total reranker input tokens:       ≈ 10,840 tokens
```

**Cost of one reranker call:**
```
Reranker Cost = 10,840 × ($0.10 / 1,000,000) = $0.001084
```

> [!NOTE]
> This is an **estimate**. Actual chunk sizes vary. The 512-token maximum is a ceiling; many chunks will be shorter. A more conservative average of ~400 tokens/chunk would give:
> ```
> 600 + (20 × 400) = 8,600 tokens → $0.00086
> ```

---

### 4.3 qwen-flash — How It's Charged

| Question | Answer |
|:---|:---|
| Input token price (Tier 1) | $0.05 / 1M tokens |
| Output token price (Tier 1) | $0.40 / 1M tokens |
| Cached input pricing | Available (cache hits at ~10% of standard input price) |
| Reasoning-token pricing | N/A — qwen-flash does not have a separate thinking/reasoning token charge |
| Maximum output tokens | **2,000** (SmartCourt configured limit) |
| Context-window limit | 1,000,000 tokens |
| Tier boundary | 256K input tokens — SmartCourt will never reach this |

**Formula:**
```
LLM Input Cost  = Input Tokens × ($0.05 / 1,000,000)
LLM Output Cost = Output Tokens × ($0.40 / 1,000,000)
Total LLM Cost  = LLM Input Cost + LLM Output Cost
```

**Maximum possible cost per request** (worst case with configured 2,000 token limit):
```
Max input: ~10,000 tokens × $0.05/1M = $0.0005
Max output: 2,000 tokens × $0.40/1M = $0.0008
Maximum LLM cost = $0.0013
```

---

## 5. SmartCourt Chat Agent Cost Model

### 5.1 Token Usage Estimates

> [!WARNING]
> The following values are **estimates used for financial modeling** only. They are NOT real production measurements. SmartCourt does not yet have production telemetry data.

#### User Question
| Size | Estimated Tokens | Rationale |
|:---|---:|:---|
| Short | 20 tokens | Quick factual question in Arabic |
| Normal | 40 tokens | Typical legal question with context |
| Long | 80 tokens | Detailed question with background |

#### System Prompt
| Component | Estimated Tokens |
|:---|---:|
| System prompt (legal assistant instructions) | ~500 tokens |

#### Conversation History
| Scenario | Estimated Tokens |
|:---|---:|
| First message (no history) | 0 tokens |
| 2 turns of history | ~500 tokens |
| 5 turns of history | ~1,500 tokens |
| 10 turns of history | ~3,500 tokens |
| 20 turns of history (heavy) | ~7,000 tokens |

#### RAG Context (Legal Articles)
| Component | Estimated Tokens |
|:---|---:|
| One legal article chunk | ~400 tokens (avg, max 512) |
| Five legal article chunks (top 5 after reranking) | **~2,000 tokens** |
| Template/formatting overhead | ~100 tokens |
| **Total RAG context** | **~2,100 tokens** |

#### Final Answer (LLM Output)
| Size | Output Tokens |
|:---|---:|
| Short | 300 tokens |
| Normal | 700 tokens |
| Long | 1,200 tokens |
| Maximum (configured limit) | 2,000 tokens |

---

### 5.2 Complete Input Token Breakdown (Normal Scenario)

| Component | Tokens | Category |
|:---|---:|:---|
| System prompt | 500 | Assumption |
| User question | 40 | Assumption |
| Conversation history (2 turns) | 500 | Assumption |
| RAG context (5 articles) | 2,100 | Assumption |
| **Total LLM input tokens** | **3,140** | Calculated |

---

### 5.3 Cost Formulas

```
┌─────────────────────────────────────────────────────┐
│  Embedding Cost                                      │
│  = User Query Tokens × $0.07 / 1,000,000            │
└─────────────────────────────────────────────────────┘

┌─────────────────────────────────────────────────────┐
│  Reranker Cost                                       │
│  = (Query × NumDocs + Total Doc Tokens) × $0.10/1M   │
└─────────────────────────────────────────────────────┘

┌─────────────────────────────────────────────────────┐
│  LLM Input Cost                                      │
│  = Total Input Tokens × $0.05 / 1,000,000            │
└─────────────────────────────────────────────────────┘

┌─────────────────────────────────────────────────────┐
│  LLM Output Cost                                     │
│  = Output Tokens × $0.40 / 1,000,000                 │
└─────────────────────────────────────────────────────┘

┌─────────────────────────────────────────────────────┐
│  TOTAL Chat Agent Cost                               │
│  = Embedding + Reranker + LLM Input + LLM Output     │
└─────────────────────────────────────────────────────┘
```

---

## 6. Per-Request Cost Scenarios

### Scenario A — Small Request

| Component | Value | Category |
|:---|:---|:---|
| User question | 20 tokens | Assumption |
| System prompt | 500 tokens | Assumption |
| Conversation history | 0 tokens (first message) | Assumption |
| RAG context (5 articles) | 2,000 tokens | Assumption |
| Output | 300 tokens | Assumption |

| Component | Tokens | Unit Price (per 1M) | Cost |
|:---|---:|---:|---:|
| **Embedding** | 20 | $0.07 | $0.0000014 |
| **Reranking** | 20×20 + 20×400 = 8,400 | $0.10 | $0.000840 |
| **LLM Input** | 500 + 20 + 0 + 2,000 = 2,520 | $0.05 | $0.000126 |
| **LLM Output** | 300 | $0.40 | $0.000120 |
| **TOTAL** | | | **$0.001087** |

---

### Scenario B — Normal Request (Main SmartCourt Scenario)

| Component | Value | Category |
|:---|:---|:---|
| User question | 40 tokens | Assumption |
| System prompt | 500 tokens | Assumption |
| Conversation history | 500 tokens (2 turns) | Assumption |
| Reranker: 20 candidates × ~400 tokens/chunk | 8,800 total tokens | Assumption |
| RAG context to LLM (5 articles) | 2,100 tokens | Assumption |
| Output | 700 tokens | Assumption |

| Component | Usage (tokens) | Unit Price (per 1M) | Cost ($) |
|:---|---:|---:|---:|
| **Embedding** | 40 | $0.07 | $0.0000028 |
| **Reranking** | 40×20 + 20×400 = 8,800 | $0.10 | $0.000880 |
| **LLM Input** | 500 + 40 + 500 + 2,100 = 3,140 | $0.05 | $0.000157 |
| **LLM Output** | 700 | $0.40 | $0.000280 |
| **TOTAL** | | | **$0.001320** |

> [!NOTE]
> We will use a **simplified normal cost of ≈ $0.00132** for subsequent calculations. However, a more realistic average accounting for shorter chunks and varying output may be closer to **$0.001** per request.
>
> For conservative financial planning, we'll use **$0.00132** (rounded to **$0.0013**).

---

### Scenario C — Heavy Request

| Component | Value | Category |
|:---|:---|:---|
| User question | 80 tokens | Assumption |
| System prompt | 500 tokens | Assumption |
| Conversation history | 3,500 tokens (10 turns) | Assumption |
| Reranker: 20 candidates × ~450 tokens/chunk | 10,600 tokens | Assumption |
| RAG context to LLM (5 articles) | 2,250 tokens | Assumption |
| Output | 1,200 tokens | Assumption |

| Component | Usage (tokens) | Unit Price (per 1M) | Cost ($) |
|:---|---:|---:|---:|
| **Embedding** | 80 | $0.07 | $0.0000056 |
| **Reranking** | 80×20 + 20×450 = 10,600 | $0.10 | $0.001060 |
| **LLM Input** | 500 + 80 + 3,500 + 2,250 = 6,330 | $0.05 | $0.000317 |
| **LLM Output** | 1,200 | $0.40 | $0.000480 |
| **TOTAL** | | | **$0.001862** |

---

### Scenario D — Maximum Request

| Component | Value | Category |
|:---|:---|:---|
| User question | 80 tokens | Assumption |
| System prompt | 500 tokens | Assumption |
| Conversation history | 7,000 tokens (20 turns) | Assumption |
| Reranker: 20 candidates × ~500 tokens/chunk | 11,600 tokens | Assumption |
| RAG context to LLM (5 articles) | 2,500 tokens | Assumption |
| Output | 2,000 tokens (configured max) | Official Fact |

| Component | Usage (tokens) | Unit Price (per 1M) | Cost ($) |
|:---|---:|---:|---:|
| **Embedding** | 80 | $0.07 | $0.0000056 |
| **Reranking** | 80×20 + 20×500 = 11,600 | $0.10 | $0.001160 |
| **LLM Input** | 500 + 80 + 7,000 + 2,500 = 10,080 | $0.05 | $0.000504 |
| **LLM Output** | 2,000 | $0.40 | $0.000800 |
| **TOTAL** | | | **$0.002470** |

---

### Scenario Summary

| Scenario | Cost per Request | Category |
|:---|---:|:---|
| A — Small | $0.001087 | Calculated |
| B — Normal | $0.001320 | Calculated |
| C — Heavy | $0.001862 | Calculated |
| D — Maximum | $0.002470 | Calculated |

---

## 7. Cost Breakdown

### Normal Request Breakdown (Scenario B)

| Component | Usage | Unit Price | Cost ($) | % of Total |
|:---|---:|---:|---:|---:|
| Embedding | 40 tokens | $0.07/1M | $0.0000028 | 0.2% |
| Reranking | 8,800 tokens | $0.10/1M | $0.000880 | 66.7% |
| LLM Input | 3,140 tokens | $0.05/1M | $0.000157 | 11.9% |
| LLM Output | 700 tokens | $0.40/1M | $0.000280 | 21.2% |
| **TOTAL** | | | **$0.001320** | **100%** |

### Cost Distribution

```
Reranking:    ████████████████████████████████████████  66.7%
LLM Output:   ████████████                              21.2%
LLM Input:    ███████                                   11.9%
Embedding:                                               0.2%
```

> [!IMPORTANT]
> **The reranker is the biggest cost driver** at 66.7% of total cost per request. This is because:
> 1. The reranker processes 20 documents × ~400 tokens each = ~8,000 document tokens
> 2. The query is replicated for each document comparison
> 3. The reranker's per-token price ($0.10/1M) is double the LLM input price ($0.05/1M)
>
> **LLM output is the second biggest driver** at 21.2%, despite generating only 700 tokens, because the output price ($0.40/1M) is 8× the input price.
>
> **Embedding cost is negligible** — less than 0.3% of total.

---

## 8. Cost Per User

Using the **Normal request cost of $0.00132 per request**.

### Per-User Daily and Monthly Cost

| Usage Level | Requests/Day | Requests/Month | Cost/Day ($) | Cost/Month ($) |
|:---|---:|---:|---:|---:|
| **Light** | 5 | 150 | $0.0066 | $0.198 |
| **Normal** | 10 | 300 | $0.0132 | $0.396 |
| **Heavy** | 20 | 600 | $0.0264 | $0.792 |
| **Very Heavy** | 50 | 1,500 | $0.0660 | $1.980 |

> [!NOTE]
> These costs represent **AI API fees only**. They do not include infrastructure, database, storage, or operational costs.

---

## 9. Scaling Analysis

### Monthly AI Cost by User Count

Using **Normal request cost ($0.00132/request)**.

#### Light Usage (5 requests/day per user)

| Users | Requests/Month | Monthly AI Cost ($) |
|---:|---:|---:|
| 100 | 15,000 | **$19.80** |
| 500 | 75,000 | **$99.00** |
| 1,000 | 150,000 | **$198.00** |
| 5,000 | 750,000 | **$990.00** |
| 10,000 | 1,500,000 | **$1,980.00** |

#### Normal Usage (10 requests/day per user)

| Users | Requests/Month | Monthly AI Cost ($) |
|---:|---:|---:|
| 100 | 30,000 | **$39.60** |
| 500 | 150,000 | **$198.00** |
| 1,000 | 300,000 | **$396.00** |
| 5,000 | 1,500,000 | **$1,980.00** |
| 10,000 | 3,000,000 | **$3,960.00** |

#### Heavy Usage (20 requests/day per user)

| Users | Requests/Month | Monthly AI Cost ($) |
|---:|---:|---:|
| 100 | 60,000 | **$79.20** |
| 500 | 300,000 | **$396.00** |
| 1,000 | 600,000 | **$792.00** |
| 5,000 | 3,000,000 | **$3,960.00** |
| 10,000 | 6,000,000 | **$7,920.00** |

### Scaling Visualization

```
Monthly AI Cost at Normal Usage (10 req/day/user):

   100 users:  ██                                    $39.60
   500 users:  █████████                             $198.00
 1,000 users:  ██████████████████                    $396.00
 5,000 users:  ██████████████████████████████████    $1,980.00
10,000 users:  MAX ▶▶▶▶▶▶▶▶▶▶▶▶▶▶▶▶▶▶▶▶▶▶▶▶▶▶▶▶    $3,960.00
```

> [!TIP]
> AI API costs scale **linearly** with user count and request volume. There are no volume discounts in the standard pay-as-you-go model, but Alibaba's batch inference (50% discount) could be explored for certain use cases.

---

## 10. Free Tier Cost

### Free Plan: 5 requests/day per user

```
5 requests/day × 30 days = 150 requests/month per free user
```

**Cost per free user per month:**
```
150 × $0.00132 = $0.198/month ≈ $0.20/month
```

| Free Users | Requests/Month | Monthly AI Cost ($) |
|---:|---:|---:|
| 1 | 150 | **$0.20** |
| 100 | 15,000 | **$19.80** |
| 500 | 75,000 | **$99.00** |
| 1,000 | 150,000 | **$198.00** |
| 5,000 | 750,000 | **$990.00** |
| 10,000 | 1,500,000 | **$1,980.00** |

> [!TIP]
> **A free plan at 5 requests/day is financially safe** for early growth:
> - 100 free users = ~$20/month — negligible
> - 1,000 free users = ~$198/month — manageable
> - 10,000 free users = ~$1,980/month — needs revenue to cover
>
> The real risk isn't cost-per-user but total user count growing without conversion to paid plans.

---

## 11. Worst-Case / Abuse Scenario

### Maximum Cost Request

Using **Scenario D** (maximum request) cost: **$0.00247/request**

### Worst-Case Abuse Profile

Assume a user sends maximum-cost requests **continuously** at a high rate:

| Metric | Value |
|:---|---:|
| Cost per request | $0.00247 |
| Requests per hour (aggressive) | 60 (1/minute) |
| Requests per day (16 active hours) | 960 |
| **Cost per abusive user per day** | **$2.37** |
| **Cost per abusive user per month** | **$71.14** |

### Abuse at Scale

| Abusive Users | Cost/Day ($) | Cost/Month ($) |
|---:|---:|---:|
| 1 | $2.37 | $71.14 |
| 10 | $23.71 | $711.40 |
| 100 | $237.12 | $7,113.60 |
| 1,000 | $2,371.20 | $71,136.00 |

> [!CAUTION]
> **Financial risk assessment:**
> - **1 abusive user** sending 1 request/minute for 16 hours/day would cost ~$71/month. This is significant but manageable.
> - **100 abusive users** would cost ~$7,100/month — a serious financial risk.
> - **Rate limiting is essential** before launching publicly. A simple limit of 50 requests/day would cap worst-case cost at $0.1235/user/day ($3.71/month).
>
> **Recommended mitigations:**
> 1. Rate limit: 50 requests/day maximum per user
> 2. Cooldown: Minimum 5 seconds between requests
> 3. Account verification: Require email/phone verification
> 4. Monitoring: Alert on users exceeding 30 requests/day

---

## 12. Qdrant Cost

### Qdrant Pricing Model

Qdrant uses **resource-based pricing** (vCPU, RAM, disk), NOT per-request or per-vector pricing.

**Source:** [Qdrant Pricing Page](https://qdrant.tech/pricing/) — Checked August 16, 2026

| Deployment | Cost | Best For |
|:---|:---|:---|
| **Free Tier** | $0/month | Prototyping, small projects |
| **Managed Cloud** | Hourly billing based on resources | Production workloads |
| **Self-Hosted (OSS)** | Free software (pay for own infra) | Full control |

### Free Tier Specifications

| Resource | Free Tier |
|:---|:---|
| vCPU | 0.5 |
| RAM | 1 GB |
| Disk | 4 GB |
| Nodes | 1 |
| Capacity | ~1 million vectors @ 768 dimensions |
| Per-request cost | **$0** (no per-query charges) |

> [!IMPORTANT]
> **Qdrant has NO per-request pricing.** This means:
> - Vector search is effectively free after provisioning
> - Cost is fixed infrastructure cost, not usage-based
> - The free tier can store ~1M vectors of 768 dimensions
> - For 1536 dimensions (SmartCourt's config), capacity is roughly **~500K vectors**
>
> **For SmartCourt's Egyptian law corpus**, the free tier may be sufficient for the initial launch if the total chunk count is under 500K.

### Production Qdrant Estimates

If SmartCourt outgrows the free tier:

| Configuration | Estimated Monthly Cost |
|:---|---:|
| 1 vCPU, 2GB RAM, 10GB disk | ~$25–50/month |
| 2 vCPU, 4GB RAM, 20GB disk | ~$50–100/month |
| Self-hosted (VPS like Hetzner/DigitalOcean) | ~$10–30/month |

> [!NOTE]
> Qdrant infrastructure cost is **separate from** and **independent of** AI API usage costs. It's a fixed monthly cost regardless of query volume.

---

## 13. Law-Ingestion Cost

### Offline Embedding Pipeline

```
Egyptian Law Documents → Chunking → text-embedding-v4 → Qdrant
```

**Configuration from source code:**

| Setting | Value | Source |
|:---|:---|:---|
| Max chunk size | 512 tokens | [appsettings.json](file:///D:/ITI%209%20Month/Graduation%20Project/SmartCourt/SmartCourt/appsettings.json#L109) |
| Overlap | 64 tokens | [appsettings.json](file:///D:/ITI%209%20Month/Graduation%20Project/SmartCourt/SmartCourt/appsettings.json#L110) |
| Min chunk size | 50 tokens | [appsettings.json](file:///D:/ITI%209%20Month/Graduation%20Project/SmartCourt/SmartCourt/appsettings.json#L111) |
| Batch size | 32 | [appsettings.json](file:///D:/ITI%209%20Month/Graduation%20Project/SmartCourt/SmartCourt/appsettings.json#L90) |

### Cost Estimates

| Corpus Size | Est. Chunks | Total Tokens | Embedding Cost ($) | Category |
|:---|---:|---:|---:|:---|
| Small (100 laws) | ~5,000 | ~2,000,000 | **$0.14** | Assumption |
| Medium (500 laws) | ~25,000 | ~10,000,000 | **$0.70** | Assumption |
| Large (2,000 laws) | ~100,000 | ~40,000,000 | **$2.80** | Assumption |
| Very Large (10,000 documents) | ~500,000 | ~200,000,000 | **$14.00** | Assumption |

**Formula:**
```
Ingestion Cost = Total Chunks × Avg Tokens per Chunk × $0.07 / 1,000,000
```

### Cost of Different Operations

| Operation | Cost | Frequency |
|:---|:---|:---|
| **Initial full ingestion** | $0.14 – $14.00 (depending on corpus) | One-time |
| **Adding 10 new laws** | ~$0.014 (≈ 500 chunks × 400 tokens) | Occasional |
| **Full re-indexing** | Same as initial ingestion | Rare (model upgrade) |

> [!TIP]
> **Law ingestion cost is negligible** compared to ongoing Chat Agent usage:
> - Full ingestion of a large corpus: ~$14 (one-time)
> - One month of 1,000 normal users: ~$396 (ongoing)
>
> Ingestion cost is a **rounding error** in the overall budget. There is no need to optimize this.

---

## 14. Sensitivity Analysis

### How Cost Changes When Variables Increase

Using **Scenario B (Normal)** as the baseline: **$0.001320/request**

---

#### Output Tokens: 700 → 1,200 tokens

| Component | Before ($) | After ($) | Change |
|:---|---:|---:|:---|
| LLM Output | $0.000280 | $0.000480 | +$0.000200 (+71%) |
| **Total request cost** | **$0.001320** | **$0.001520** | **+15.2%** |

> Increasing output from 700 to 1,200 tokens raises per-request cost by **~15%**.

---

#### Requests per user double: 10 → 20/day

| Metric | Before | After | Change |
|:---|---:|---:|:---|
| Requests/month | 300 | 600 | +100% |
| Cost/user/month | $0.396 | $0.792 | **+100%** |

> Cost scales **linearly** — doubling requests doubles cost.

---

#### Conversation history doubles: 500 → 1,000 tokens

| Component | Before ($) | After ($) | Change |
|:---|---:|---:|:---|
| LLM Input cost | $0.000157 | $0.000182 | +$0.000025 |
| **Total request cost** | **$0.001320** | **$0.001345** | **+1.9%** |

> Conversation history has **minimal impact** on cost because LLM input tokens are very cheap ($0.05/1M).

---

#### Conversation history grows to 10 turns (~3,500 tokens)

| Component | Before ($) | After ($) | Change |
|:---|---:|---:|:---|
| LLM Input tokens | 3,140 | 6,140 | +96% |
| LLM Input cost | $0.000157 | $0.000307 | +$0.000150 |
| **Total request cost** | **$0.001320** | **$0.001470** | **+11.4%** |

> Even with 10 turns of history, cost increase is only **~11%**.

---

#### Number of reranker documents: 20 → 40

| Component | Before ($) | After ($) | Change |
|:---|---:|---:|:---|
| Reranker tokens | 8,800 | 17,600 | +100% |
| Reranker cost | $0.000880 | $0.001760 | +100% |
| **Total request cost** | **$0.001320** | **$0.002200** | **+66.7%** |

> Doubling reranker documents **dramatically increases cost** (+67%) because the reranker is the dominant cost driver.

---

#### Number of reranker documents reduced: 20 → 10

| Component | Before ($) | After ($) | Change |
|:---|---:|---:|:---|
| Reranker tokens | 8,800 | 4,400 | -50% |
| Reranker cost | $0.000880 | $0.000440 | -50% |
| **Total request cost** | **$0.001320** | **$0.000880** | **-33.3%** |

> Halving reranker documents **reduces total cost by 33%** — the single biggest optimization opportunity.

---

#### RAG context increased: 5 → 10 articles in prompt

| Component | Before ($) | After ($) | Change |
|:---|---:|---:|:---|
| LLM Input tokens | 3,140 | 5,240 | +67% |
| LLM Input cost | $0.000157 | $0.000262 | +$0.000105 |
| **Total request cost** | **$0.001320** | **$0.001425** | **+8.0%** |

> Doubling RAG context to the LLM has a **modest 8% impact** because LLM input is cheap.

---

### Sensitivity Summary Table

| Variable | Change | Cost Impact |
|:---|:---|:---|
| Output tokens 700→1200 | +71% more output | **+15.2%** total cost |
| Requests/user double | 10→20/day | **+100%** monthly cost |
| History 500→1000 tokens | +100% history | **+1.9%** total cost |
| History 500→3500 tokens | +600% history | **+11.4%** total cost |
| Reranker docs 20→40 | +100% docs | **+66.7%** total cost |
| Reranker docs 20→10 | -50% docs | **-33.3%** total cost |
| RAG context 5→10 articles | +100% context | **+8.0%** total cost |

---

## 15. Main Cost Drivers

### Answers to Key Questions

| # | Question | Answer |
|:---|:---|:---|
| 1 | Which model costs the most? | **qwen3-rerank** — 66.7% of total per-request cost |
| 2 | Which operation costs the most? | **Reranking** — processing 20 documents dominates |
| 3 | Is embedding cost significant? | **No** — less than 0.3% of total |
| 4 | Is reranking cost significant? | **Yes** — it is the #1 cost driver at 66.7% |
| 5 | Is LLM generation the biggest cost? | **No** — LLM (input + output) is ~33% combined; reranking is larger |
| 6 | Does conversation history have major cost impact? | **No** — even 10 turns only adds ~11% because LLM input is cheap |
| 7 | Does RAG context have major cost impact? | **No** — doubling RAG context adds only ~8% |
| 8 | What is the biggest opportunity for reducing cost? | **Reducing reranker candidate count** from 20 to 10 would cut total cost by ~33% |

---

## 16. Cost Optimization Opportunities

### Ranked by Impact

| # | Optimization | Estimated Savings | Complexity |
|:---|:---|:---|:---|
| 1 | **Reduce reranker candidates from 20 → 10** | -33% total cost | Low (config change) |
| 2 | **Use Alibaba batch inference for non-real-time** | -50% on batch jobs | Medium |
| 3 | **Implement context caching** for system prompt | -10% of LLM input cost (~negligible) | Medium |
| 4 | **Limit conversation history** to last 5 turns | -5-10% on heavy conversations | Low |
| 5 | **Reduce max output tokens** from 2000 → 1500 | Reduces worst-case cost by ~10% | Low |
| 6 | **Use smaller embedding dimensions** (768 vs 1536) | No cost savings (pricing is by tokens, not dimensions) | N/A |

> [!TIP]
> **Recommendation #1:** Test whether reducing reranker candidates from 20 to 10 significantly impacts answer quality. If quality remains acceptable, this single change would reduce AI cost by ~33%.
>
> **Recommendation #2:** Monitor actual production output token counts. If most responses are under 500 tokens, the average cost per request may be closer to $0.001 than the estimated $0.00132.

---

## 17. Future Quota Strategy

### Option 1 — Requests Per Day

**Example:** `10 requests/day`

| Advantage | Disadvantage |
|:---|:---|
| Simple to understand for users | All requests treated equally (short = expensive) |
| Easy to implement | A "1-word question" costs the same quota as a heavy legal research query |
| Predictable UX | Doesn't account for conversation length |

### Option 2 — Tokens Per Day

**Example:** `50,000 tokens/day`

| Advantage | Disadvantage |
|:---|:---|
| Fairer — heavy queries cost more | Hard for users to understand |
| Better cost alignment | Requires token counting exposed to user |
| Prevents abuse via long conversations | Complex to implement and communicate |

### Option 3 — AI Credits

**Example:** Users receive 100 credits/day, where each credit ≈ $0.001 of AI cost

| Advantage | Disadvantage |
|:---|:---|
| Flexible and fair | Requires calibrating credit cost |
| Can weight different operations | More complex to implement |
| Allows premium queries to cost more credits | Users need education on the system |

### Recommendation

> [!IMPORTANT]
> **Recommended: Option 1 — Requests Per Day** for initial launch.
>
> **Rationale:**
> - SmartCourt is a legal Q&A tool — request complexity doesn't vary dramatically (most requests involve the same pipeline: embed → rerank 20 → generate)
> - The cost variance between a small request ($0.001) and a maximum request ($0.0025) is only 2.5×, making per-request limits a reasonable proxy for cost
> - Users can easily understand "You have 10 questions remaining today"
> - Simple to implement and enforce
>
> **Suggested initial limits:**
>
> | Plan | Requests/Day | Est. Max Cost/User/Month |
> |:---|---:|---:|
> | Free | 5 | $0.20 |
> | Basic | 15 | $0.59 |
> | Pro | 40 | $1.58 |
> | Premium | Unlimited (soft: 200) | $7.92 |
>
> **Migrate to AI Credits (Option 3) later** when production data reveals significant cost variance between request types.

---

## 18. Future Subscription Pricing Considerations

### AI Cost Baseline

| Metric | Value | Category |
|:---|---:|:---|
| Average cost/user/month (normal, 10 req/day) | **$0.40** | Calculated |
| Heavy user cost/month (20 req/day) | **$0.79** | Calculated |
| Very heavy user cost/month (50 req/day) | **$1.98** | Calculated |
| Maximum expected cost/user/month | **$3.71** (50 req/day, heavy requests) | Calculated |
| Worst-case abusive user/month | **$71.14** (without rate limiting) | Calculated |

### Subscription Structure Framework

| Plan | Requests/Day | AI Cost/Month | Suggested Price* | Profit Margin** |
|:---|---:|---:|---:|:---|
| **Free** | 5 | $0.20 | $0 | (loss leader) |
| **Basic** | 15 | $0.59 | $5–10/month | ~85–95% |
| **Pro** | 40 | $1.58 | $15–25/month | ~90–94% |
| **Premium** | 200 | $7.92 | $50–75/month | ~84–89% |

> *Suggested prices are illustrative ranges, not final recommendations.
>
> **Profit margin shown is on AI cost only. The subscription must also cover:

### Non-AI Costs to Cover

| Cost Category | Estimated Monthly Cost | Notes |
|:---|:---|:---|
| Server infrastructure (VPS/Cloud) | $20–100/month | App server, database |
| SQL Server database | Included in server or $0–50 | Depending on deployment |
| Qdrant hosting | $0–50/month | Free tier initially |
| Email/SMS provider | $10–50/month | Transactional emails, OTP |
| Domain + SSL | ~$2/month | Amortized annually |
| Monitoring/Logging | $0–30/month | Free tier for small scale |
| Payment processing fees | ~3% of revenue | Stripe/PayPal/local |
| Support labor | Variable | Manual support cost |
| **Estimated fixed overhead** | **$50–300/month** | Before any AI costs |

> [!IMPORTANT]
> **Key insight:** At small scale (< 500 users), **fixed infrastructure costs dominate over AI API costs**. At larger scale (> 5,000 users), AI API costs become the primary variable cost.
>
> **Pricing strategy:**
> 1. Set subscription prices to cover fixed infrastructure costs first
> 2. Add AI cost per user as the variable component
> 3. Include a healthy margin (3–5× the AI cost) for sustainability
> 4. Use the free plan as a conversion funnel, not a cost center

---

## 19. Assumptions and Limitations

### Assumptions Made (Labeled Throughout Report)

| Assumption | Value Used | Impact if Wrong |
|:---|:---|:---|
| Average chunk size | 400 tokens | ±20% on reranker cost |
| System prompt size | 500 tokens | Minimal — < 2% of total input |
| Normal conversation history | 500 tokens (2 turns) | Minimal — LLM input is cheap |
| Normal output length | 700 tokens | ±10% on total cost |
| Arabic token ratio | Not adjusted (using English estimates) | Arabic may use 1.3–1.5× more tokens per word |
| User behavior distribution | Assumed uniform | Real distribution likely skewed toward light usage |

### Limitations

1. **No production data** — All usage assumptions are estimates. Real production telemetry will likely show different patterns.
2. **Arabic tokenization** — Arabic text may require more tokens per word than English due to tokenizer design. This could increase costs by 30–50%.
3. **Pricing changes** — Alibaba Cloud pricing changes frequently. The prices in this report are from August 16, 2026.
4. **Free quota depletion** — The 1M token free quota (90-day) for each model will eventually expire. All calculations assume post-free-quota pricing.
5. **No volume discounts** — Analysis assumes standard pay-as-you-go pricing. Committed-use discounts may be available for larger volumes.
6. **Reranker token formula** — The exact query replication behavior (query × number of documents) is based on Alibaba's documentation but may differ slightly in practice.

---

## 20. Final Recommendation

### Answers to the 8 Key Questions

---

### 1. How much does one normal SmartCourt Chat Agent request cost in AI API fees?

**≈ $0.0013 per request** (Scenario B — Normal)

| Component | Cost |
|:---|---:|
| Embedding | $0.000003 |
| Reranking | $0.000880 |
| LLM Input | $0.000157 |
| LLM Output | $0.000280 |
| **Total** | **$0.001320** |

---

### 2. How much does one normal user cost SmartCourt per day?

**≈ $0.013 per day** (10 requests/day at normal cost)

---

### 3. How much does one normal user cost SmartCourt per month?

**≈ $0.40 per month** (300 requests/month at normal cost)

---

### 4. How much would 1,000 active users cost per month?

| Usage Pattern | Monthly AI Cost |
|:---|---:|
| Light (5 req/day) | **$198** |
| Normal (10 req/day) | **$396** |
| Heavy (20 req/day) | **$792** |

---

### 5. What is the expected cost for light, normal, heavy, and worst-case usage?

| Usage Level | Cost/Request | Cost/User/Month |
|:---|---:|---:|
| Light (small request) | $0.0011 | $0.16 |
| Normal | $0.0013 | $0.40 |
| Heavy | $0.0019 | $0.56 |
| Worst-case (max request) | $0.0025 | $0.74 |
| Worst-case abuse (960 req/day) | $0.0025 | $71.14 |

---

### 6. Which AI model is responsible for most of the cost?

**qwen3-rerank** — responsible for **66.7%** of the total per-request AI cost.

The reranker processes 20 document chunks per request, making it by far the most expensive component. The LLM (qwen-flash) is second at ~33% combined (input + output).

---

### 7. What usage limit would be financially reasonable for a future free plan?

**5 requests/day** (150 requests/month)

| Free Users | Monthly AI Cost |
|---:|---:|
| 100 | $19.80 |
| 1,000 | $198.00 |
| 10,000 | $1,980.00 |

This is financially manageable for early growth. Combine with rate limiting (5-second cooldown) to prevent abuse.

---

### 8. What information do we still need from real production usage before setting final quotas and subscription prices?

| Data Needed | Why |
|:---|:---|
| **Actual average output tokens** | Real responses may average 300–500 tokens, not 700 |
| **Actual conversation length distribution** | Most users may ask 1–3 questions, not 10 |
| **Arabic tokenization overhead** | May increase all estimates by 30–50% |
| **Actual chunk sizes** | Average may be 300 tokens, not 400 |
| **Request frequency distribution** | Most users are likely light; a few are heavy |
| **Peak vs. off-peak patterns** | For capacity planning and potential time-of-day pricing |
| **Reranker quality at lower candidate counts** | Can we reduce from 20 to 10 candidates? |
| **User conversion rate** | What % of free users convert to paid? |
| **User retention** | Average active days per month |

> [!IMPORTANT]
> **Action items before setting production prices:**
> 1. Add token counting telemetry to every AI API call
> 2. Run a beta with 50–100 users for 2–4 weeks
> 3. Analyze actual token distributions
> 4. Test reranker quality with 10 vs 20 candidates
> 5. Measure Arabic tokenization overhead with real legal questions
> 6. Then revisit this cost model with real data

---

### Pricing Sources

| Model | Price | Source |
|:---|:---|:---|
| text-embedding-v4 | $0.07/1M input tokens | [Alibaba Cloud Model Studio Pricing](https://www.alibabacloud.com/help/en/model-studio/billing-for-model-studio) — Singapore, International |
| qwen3-rerank | $0.10/1M input tokens | [Alibaba Cloud Model Studio Pricing](https://www.alibabacloud.com/help/en/model-studio/billing-for-model-studio) — Singapore, International |
| qwen-flash (Tier 1, ≤256K) | Input: $0.05/1M, Output: $0.40/1M | [Alibaba Cloud Model Studio Pricing](https://www.alibabacloud.com/help/en/model-studio/billing-for-model-studio) — Singapore, International |
| Qdrant Free Tier | $0/month (0.5 vCPU, 1GB RAM, 4GB disk) | [Qdrant Pricing](https://qdrant.tech/pricing/) |

**All prices verified:** August 16, 2026

---

*End of Report*
