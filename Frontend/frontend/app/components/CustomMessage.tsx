"use client";

import { Markdown } from "@copilotkit/react-ui";

interface MessageProps {
    message?: any;
    inProgress?: boolean;
}

export function CustomAssistantMessage({ message, inProgress = false }: MessageProps) {
    if (!message) return null;
    const timestamp = new Date(message.createdAt || Date.now()).toLocaleString("fr-FR", {
        day: "2-digit",
        month: "2-digit",
        year: "numeric",
        hour: "2-digit",
        minute: "2-digit",
    });

    let messageContent = (message as any).content || "";
    let isEmpty = false;
    if (messageContent == null|| messageContent.trim().length === 0) {
        const funct = message.toolCalls?.[0]?.function;
        if (funct == null) {
            messageContent = "Thinking ...";
        } else {
            messageContent = "🔎 " + funct.name + " " + funct.arguments;
            isEmpty = true;
        }

    }

    if (isEmpty) {
        return null;
    }

    return (
        <div
            style={{
                display: "flex",
                gap: "12px",
                padding: "12px 0",
                alignItems: "flex-start",
            }}
        >
            <img
                src="/PP/architect.png"
                alt="Power Platform Architect"
                style={{
                    width: "40px",
                    height: "40px",
                    borderRadius: "50%",
                    flexShrink: 0,
                    objectFit: "cover",
                    border: "2px solid #4F8AD4",
                }}
                onError={(e) => {
                    console.error("Failed to load architect image");
                    e.currentTarget.style.display = "none";
                }}
            />
            <div style={{ flex: 1, minWidth: 0 }}>
                <div
                    style={{
                        display: "flex",
                        alignItems: "center",
                        gap: "12px",
                        marginBottom: "6px",
                    }}
                >
                    <span
                        style={{
                            fontSize: "13px",
                            fontWeight: 600,
                            color: "#4F8AD4",
                        }}
                    >
                        Assistant
                    </span>
                    <span
                        style={{
                            fontSize: "12px",
                            color: "#9AA0A6",
                        }}
                    >
                        {timestamp}
                    </span>
                </div>
                <div
                    style={{
                        background: "#16212D",
                        padding: "12px 16px",
                        borderRadius: "8px",
                        border: "1px solid #4F8AD4",
                        color: "#E8EAED",
                        lineHeight: "1.2",
                        wordWrap: "break-word",
                        opacity: inProgress ? 0.7 : 1,
                    }}
                >

                    <Markdown content={messageContent} />

                </div>
            </div>
        </div>
    );
}

export function CustomUserMessage({ message }: MessageProps) {
    if (!message) return null;
    const timestamp = new Date(message.createdAt || Date.now()).toLocaleString("fr-FR", {
        day: "2-digit",
        month: "2-digit",
        year: "numeric",
        hour: "2-digit",
        minute: "2-digit",
    });

    const messageContent = (message as any).content || "";
    if (!messageContent.trim()) {
        return null;
    }

    return (
        <div
            style={{
                display: "flex",
                gap: "12px",
                padding: "12px 0",
                alignItems: "flex-start",
            }}
        >
            <img
                src="/PP/human.png"
                alt="User"
                style={{
                    width: "40px",
                    height: "40px",
                    borderRadius: "50%",
                    flexShrink: 0,
                    objectFit: "cover",
                    border: "2px solid #2A476C",
                }}
                onError={(e) => {
                    console.error("Failed to load user image");
                    e.currentTarget.style.display = "none";
                }}
            />
            <div style={{ flex: 1, minWidth: 0 }}>
                <div
                    style={{
                        display: "flex",
                        alignItems: "center",
                        gap: "12px",
                        marginBottom: "6px",
                    }}
                >
                    <span
                        style={{
                            fontSize: "13px",
                            fontWeight: 600,
                            color: "#7CAEED",
                        }}
                    >
                        You
                    </span>
                    <span
                        style={{
                            fontSize: "12px",
                            color: "#9AA0A6",
                        }}
                    >
                        {timestamp}
                    </span>
                </div>
                <div
                    style={{
                        background: "#1B2A3D",
                        padding: "12px 16px",
                        borderRadius: "8px",
                        border: "1px solid #2A476C",
                        color: "#E8EAED",
                        lineHeight: "1.2",
                        wordWrap: "break-word",
                    }}
                >
                    <Markdown content={messageContent} />
                </div>
            </div>
        </div>
    );
}



