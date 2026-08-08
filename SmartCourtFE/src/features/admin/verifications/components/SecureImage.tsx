import { useState, useEffect } from "react";
import { apiClient } from "../../../../api/apiClient";

interface SecureImageProps extends React.ImgHTMLAttributes<HTMLImageElement> {
  url: string;
}

export const SecureImage = ({ url, ...props }: SecureImageProps) => {
  const [objectUrl, setObjectUrl] = useState<string | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | false>(false);

  useEffect(() => {
    let active = true;

    const fetchImage = async () => {
      setLoading(true);
      setError(false);
      try {
        const response = await apiClient.get(url);

        if (active) {
          // The API returns ApiResponse<VerificationDocumentContentDto>
          // So we extract data.data.downloadUrl
          const downloadUrl = response.data?.data?.downloadUrl;
          if (downloadUrl) {
            setObjectUrl(downloadUrl);
          } else {
            setError("No downloadUrl in response");
          }
          setLoading(false);
        }
      } catch (err: any) {
        if (active) {
          setError(err.message || "Request failed");
          setLoading(false);
        }
      }
    };

    if (url) {
      fetchImage();
    }

    return () => {
      active = false;
    };
  }, [url]);

  if (loading) {
    return (
      <div className="w-full h-full bg-gray-200 dark:bg-gray-800 animate-pulse rounded-md flex items-center justify-center">
        <span className="text-gray-400 text-xs">جاري تحميل الصورة...</span>
      </div>
    );
  }

  if (error || !objectUrl) {
    return (
      <div className="w-full h-full bg-red-50 dark:bg-red-900/20 border border-red-200 dark:border-red-900 rounded-md flex flex-col items-center justify-center text-red-500 text-xs text-center p-2">
        <span>فشل تحميل الصورة</span>
        <span className="text-[10px] break-all mt-1 text-red-400 font-mono">{url}</span>
        <span className="text-[9px] break-all mt-1 text-red-600 font-bold">{error}</span>
      </div>
    );
  }

  return <img src={objectUrl} {...props} />;
};
