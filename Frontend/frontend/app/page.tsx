"use client";

import React from "react";
import { CopilotChat } from "@copilotkit/react-ui";
import "@copilotkit/react-ui/styles.css";
import { useHumanInTheLoop } from "@copilotkit/react-core";
import DiagnosticLogs from "./components/DiagnosticLogs"



export default function Page() {

    useHumanInTheLoop({
        name: "humanApprovedCommand",
        description: "Ask human for approval to run a command.",
        parameters: [
            {
                name: "command",
                type: "string",
                description: "The command to run",
                required: true,
            },
        ],
        render: ({ args, respond }) => {
            if (!respond) return <></>;
            return (
                <div>
                    <pre>{args.command}</pre>
                    <button onClick={() => respond(`Command is APPROVED`)}>Approve</button>
                    <button onClick={() => respond(`Command is DENIED`)}>Deny</button>
                </div>
            );
        },
    });

    return (
        <main style={{ display: "flex", height: "100vh", gap: "16px", padding: "16px" }}>
            <div style={{ flex: "1", display: "flex", flexDirection: "column" }}>
                <h1>Welcome to Your Dataverse Assistant</h1>
                <div style={{ flex: "1" }}>
                    <CopilotChat
                        labels={{
                            title: "Your Dataverse Assistant",
                            initial: "Hi! 👋 How can I assist you today?",
                        }}
                    />
                </div>
            </div>
            
            <div style={{ flex: "1" }}>
                <DiagnosticLogs />
            </div>
        </main>
    );
}