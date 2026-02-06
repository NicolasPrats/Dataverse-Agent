import { Providers } from "./Providers";
import "@copilotkit/react-ui/styles.css";
import "./globals.css";
import "./copilot-dark-theme.css";

export default function RootLayout({ children }: { children: React.ReactNode }) {
    return (
        <html lang="en">
            <body style={{ margin: 0, padding: 0 }}>
                <Providers>{children}</Providers>
            </body>
        </html>
    );
}