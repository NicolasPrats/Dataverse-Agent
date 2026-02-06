"use client";

import React, { useState } from "react";
import { CopilotChat } from "@copilotkit/react-ui";
import "@copilotkit/react-ui/styles.css";
import { useHumanInTheLoop } from "@copilotkit/react-core";
import DiagnosticLogs from "./components/DiagnosticLogs";
import ResizablePanel from "./components/ResizablePanel";
import { Button } from "@fluentui/react-components";
import { PanelRightRegular, DismissRegular } from "@fluentui/react-icons"



export default function Page() {
const [isPanelOpen, setIsPanelOpen] = useState(false);

    return (
        <main style={{ display: "flex", height: "100vh", background: "#0F1419" }}>
            <div style={{ flex: "1", display: "flex", flexDirection: "column", padding: "16px" }}>
                <div
                    style={{
                        display: "flex",
                        justifyContent: "space-between",
                        alignItems: "center",
                        marginBottom: "16px",
                    }}
                >
                    <div style={{ display: "flex", alignItems: "center", gap: "12px" }}>
                        <img
                            src="/Dataverse-Logo.png"
                            alt="Dataverse Logo"
                            style={{ height: "32px", width: "auto" }}
                        />
                        <h1 style={{ margin: 0, fontSize: "24px", fontWeight: 600, color: "#E8EAED" }}>
                            Dataverse Assistant
                        </h1>
                    </div>
                    <Button
                        appearance="subtle"
                        icon={isPanelOpen ? <DismissRegular /> : <PanelRightRegular />}
                        onClick={() => setIsPanelOpen(!isPanelOpen)}
                    >
                        {isPanelOpen ? "Hide" : "Show"} Diagnostics
                    </Button>
                </div>
                <div style={{ flex: "1", overflow: "hidden" }}>
                    <div
                        style={{
                            height: "100%",
                            background: "#16212D",
                            borderRadius: "8px",
                            border: "1px solid #2A476C",
                            padding: "8px",
                            boxShadow: "0 2px 8px rgba(0, 0, 0, 0.3)",
                        }}
                    >
                        <CopilotChat
                            labels={{
                                title: "Your Dataverse Assistant",
                                initial: "Hi! 👋 How can I assist you today?",
                            }}
                        />
                    </div>
                </div>
            </div>

            <ResizablePanel
                isOpen={isPanelOpen}
                onClose={() => setIsPanelOpen(false)}
                defaultWidth={33}
                minWidth={20}
                maxWidth={60}
            >
                <div
                    style={{
                        padding: "16px",
                        height: "100%",
                        background: "#0F1419",
                    }}
                >
                    <div
                        style={{
                            height: "100%",
                            background: "#16212D",
                            borderRadius: "8px",
                            border: "1px solid #2A476C",
                            padding: "16px",
                            boxShadow: "0 2px 8px rgba(0, 0, 0, 0.3)",
                        }}
                    >
                        <DiagnosticLogs />
                    </div>
                </div>
            </ResizablePanel>
        </main>
    );
}