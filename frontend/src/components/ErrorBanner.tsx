interface ErrorBannerProps {
  message: string;
}

/** Simple inline error message. */
export function ErrorBanner({ message }: ErrorBannerProps) {
  return (
    <div className="error-banner" role="alert">
      {message}
    </div>
  );
}
