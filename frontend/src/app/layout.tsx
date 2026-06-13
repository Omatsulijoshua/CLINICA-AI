import type { Metadata } from "next";
import "./globals.css";

export const metadata: Metadata = {
  title: "Clinica AI - Evidence-Based Medical Assistant",
  description: "An evidence-based, HIPAA-inspired clinical AI assistant helping you understand symptoms, lab reports, medications, and wellness guidelines.",
};

export default function RootLayout({
  children,
}: Readonly<{
  children: React.ReactNode;
}>) {
  return (
    <html lang="en" className="dark">
      <body className="antialiased bg-clinical-dark text-gray-100 overflow-hidden h-screen w-screen">
        {children}
      </body>
    </html>
  );
}
