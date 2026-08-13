import React, { useState } from "react";
import { useForm, Controller } from "react-hook-form";
import { zodResolver } from "@hookform/resolvers/zod";
import { z } from "zod";
import { useMutation, useQueryClient } from "@tanstack/react-query";
import toast from "react-hot-toast";
import { CasesApi } from "../api/casesApi";
import { FileUploadZone } from "../../../components/FileUploadZone";
import {
  LuShield,
  LuSend,
  LuLoaderCircle,
} from "react-icons/lu";

const createCaseSchema = z.object({
  governorate: z.string().min(1, "يرجى إدخال المحافظة"),
  city: z.string().min(1, "يرجى إدخال المدينة"),
  title: z.string().min(5, "عنوان القضية يجب أن يكون 5 أحرف على الأقل"),
  description: z
    .string()
    .min(20, "الرجاء تقديم وصف تفصيلي للقضية (20 حرف على الأقل)"),
  documents: z.array(z.instanceof(File)).default([]),
});

export type CreateCaseFormValues = z.infer<typeof createCaseSchema>;

export const CreateCaseForm: React.FC = () => {
  const queryClient = useQueryClient();
  const [isSubmitting, setIsSubmitting] = useState(false);

  const {
    register,
    handleSubmit,
    control,
    reset,
    formState: { errors },
  } = useForm<CreateCaseFormValues>({
    resolver: zodResolver(createCaseSchema) as any,
    defaultValues: {
      governorate: "",
      city: "",
      title: "",
      description: "",
      documents: [],
    },
  });

  const createCaseMutation = useMutation({
    mutationFn: CasesApi.createCase,
    onMutate: () => setIsSubmitting(true),
    onSuccess: () => {
      toast.success("تم تقديم القضية بنجاح!");
      queryClient.invalidateQueries({ queryKey: ["cases"] });
      reset();
    },
    onError: (error: any) => {
      console.error("Error creating case:", error);
      toast.error("حدث خطأ أثناء تقديم القضية، يرجى المحاولة مرة أخرى.");
    },
    onSettled: () => {
      setIsSubmitting(false);
    },
  });

  const onSubmit = (data: any) => {
    createCaseMutation.mutate(data);
  };

  return (
    <form
      onSubmit={handleSubmit(onSubmit)}
      className="space-y-8 bg-white dark:bg-[#1a1d23] p-8 md:p-10 rounded-xl border border-gray-200 dark:border-gray-800 shadow-sm"
    >
      {/* Section 1: Case Details */}
      <fieldset className="space-y-6">
        <legend className="text-xl font-bold text-gold border-b border-gray-200 dark:border-gray-800 pb-2 w-full mb-6 relative">
          تفاصيل القضية
          <span className="absolute bottom-0 right-0 w-16 h-[2px] bg-gold"></span>
        </legend>

        <div className="space-y-2">
          <label
            className="block text-sm font-semibold text-gray-700 dark:text-gray-300"
            htmlFor="title"
          >
            عنوان القضية (مختصر)
          </label>
          <input
            {...register("title")}
            className={`w-full bg-gray-50 dark:bg-[#121620] border ${errors.title ? "border-red-500" : "border-gray-300 dark:border-gray-700"} rounded-lg p-3 text-gray-900 dark:text-gray-200 focus:ring-2 focus:ring-gold focus:border-gold`}
            id="title"
            placeholder="مثال: نزاع تجاري حول عقد توريد"
            type="text"
          />
          {errors.title && (
            <p className="text-xs text-red-500">{errors.title.message}</p>
          )}
        </div>

        <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
          <div className="space-y-2">
            <label
              className="block text-sm font-semibold text-gray-700 dark:text-gray-300"
              htmlFor="governorate"
            >
              المحافظة
            </label>
            <input
              {...register("governorate")}
              className={`w-full bg-gray-50 dark:bg-[#121620] border ${errors.governorate ? "border-red-500" : "border-gray-300 dark:border-gray-700"} rounded-lg p-3 text-gray-900 dark:text-gray-200 focus:ring-2 focus:ring-gold focus:border-gold`}
              id="governorate"
              placeholder="أدخل المحافظة"
              type="text"
            />
            {errors.governorate && (
              <p className="text-xs text-red-500">
                {errors.governorate.message}
              </p>
            )}
          </div>

          <div className="space-y-2">
            <label
              className="block text-sm font-semibold text-gray-700 dark:text-gray-300"
              htmlFor="city"
            >
              المدينة
            </label>
            <input
              {...register("city")}
              className={`w-full bg-gray-50 dark:bg-[#121620] border ${errors.city ? "border-red-500" : "border-gray-300 dark:border-gray-700"} rounded-lg p-3 text-gray-900 dark:text-gray-200 focus:ring-2 focus:ring-gold focus:border-gold`}
              id="city"
              placeholder="أدخل المدينة"
              type="text"
            />
            {errors.city && (
              <p className="text-xs text-red-500">{errors.city.message}</p>
            )}
          </div>
        </div>

        <div className="space-y-2">
          <label
            className="block text-sm font-semibold text-gray-700 dark:text-gray-300"
            htmlFor="description"
          >
            وصف تفصيلي
          </label>
          <textarea
            {...register("description")}
            className={`w-full bg-gray-50 dark:bg-[#121620] border ${errors.description ? "border-red-500" : "border-gray-300 dark:border-gray-700"} rounded-lg p-3 text-gray-900 dark:text-gray-200 focus:ring-2 focus:ring-gold focus:border-gold min-h-[120px]`}
            id="description"
            placeholder="الرجاء تقديم سرد واضح للأحداث والوقائع الرئيسية..."
            rows={5}
          ></textarea>
          {errors.description && (
            <p className="text-xs text-red-500">{errors.description.message}</p>
          )}
        </div>
      </fieldset>

      {/* Section 2: File Upload */}
      <fieldset className="space-y-6">
        <legend className="text-xl font-bold text-gold border-b border-gray-200 dark:border-gray-800 pb-2 w-full mb-6 relative">
          المستندات الداعمة
          <span className="absolute bottom-0 right-0 w-16 h-[2px] bg-gold"></span>
        </legend>

        <Controller
          control={control}
          name="documents"
          render={({ field: { onChange, value } }) => (
            <FileUploadZone
              files={value || []}
              onChange={onChange}
              maxFiles={10}
              maxSizeMB={20}
            />
          )}
        />
        {errors.documents && (
          <p className="text-xs text-red-500">{errors.documents.message}</p>
        )}
      </fieldset>

      {/* Privacy Callout */}
      <div className="bg-amber-50 dark:bg-amber-900/10 border-r-4 border-gold p-4 rounded-l-lg flex gap-4 mt-8">
        <LuShield className="w-6 h-6 text-gold shrink-0 mt-0.5" />
        <div>
          <h4 className="text-sm font-bold text-gray-900 dark:text-white">
            حماية الخصوصية والسرية
          </h4>
          <p className="text-xs text-gray-600 dark:text-gray-400 mt-1 leading-relaxed">
            جميع المعلومات والمستندات المقدمة تخضع لسرية تامة ومحمية بموجب سياسة
            الخصوصية الخاصة بمنصة مستشار، ولا يتم مشاركتها إلا مع الفريق
            القانوني المختص.
          </p>
        </div>
      </div>

      {/* Actions */}
      <div className="flex items-center justify-end gap-4 pt-6 border-t border-gray-200 dark:border-gray-800">
        <button
          className="px-6 py-3 rounded-lg text-sm font-semibold text-gray-500 hover:text-gray-700 dark:text-gray-400 dark:hover:text-gray-200 transition-colors"
          type="button"
          onClick={() => reset()}
          disabled={isSubmitting}
        >
          إلغاء
        </button>
        <button
          className="bg-gold text-white px-8 py-3 rounded-lg text-sm font-bold flex items-center gap-2 hover:bg-gold-hover transition-colors shadow-lg shadow-gold/20 disabled:opacity-70 disabled:cursor-not-allowed"
          type="submit"
          disabled={isSubmitting}
        >
          {isSubmitting ? (
            <>
              جاري التقديم...
              <LuLoaderCircle className="w-5 h-5 animate-spin" />
            </>
          ) : (
            <>
              تقديم القضية
              <LuSend className="w-5 h-5 rotate-180" />
            </>
          )}
        </button>
      </div>
    </form>
  );
};
