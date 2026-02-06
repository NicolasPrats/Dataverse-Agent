import { NextRequest } from "next/server";

export const dynamic = "force-dynamic";

export async function GET(request: NextRequest) {
    const controller = new AbortController();
    const { signal } = controller;

    request.signal.addEventListener("abort", () => {
        controller.abort();
    });

    try {
        const response = await fetch("http://localhost:5080/diagnostics", {
            signal,
            headers: {
                Accept: "text/event-stream",
            },
        });

        if (!response.ok || !response.body) {
            return new Response("Failed to connect to diagnostics", { status: 500 });
        }

        const stream = new ReadableStream({
            async start(controller) {
                const reader = response.body!.getReader();
                const decoder = new TextDecoder();

                try {
                    while (true) {
                        const { done, value } = await reader.read();
                        if (done) break;

                        const chunk = decoder.decode(value, { stream: true });
                        controller.enqueue(new TextEncoder().encode(chunk));
                    }
                } catch (error) {
                    console.error("Stream error:", error);
                } finally {
                    controller.close();
                    reader.releaseLock();
                }
            },
        });

        return new Response(stream, {
            headers: {
                "Content-Type": "text/event-stream",
                "Cache-Control": "no-cache",
                Connection: "keep-alive",
            },
        });
    } catch (error) {
        console.error("Diagnostics proxy error:", error);
        return new Response("Proxy error", { status: 500 });
    }
}
