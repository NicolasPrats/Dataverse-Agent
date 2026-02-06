"use client";

import { CopilotKit } from "@copilotkit/react-core";
import { FluentProvider } from "@fluentui/react-components";
import { dataverseDarkTheme } from "./theme";
import { ReactNode } from "react";

interface ProvidersProps {
    children: ReactNode;
}

export function Providers({ children }: ProvidersProps) {
    return (
        <FluentProvider theme={dataverseDarkTheme}>
            <CopilotKit runtimeUrl="/api/copilotkit" agent="default">
                {children}
            </CopilotKit>
        </FluentProvider>
    );
}
