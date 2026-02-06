"use client";

import { useState, useRef, useEffect, ReactNode } from "react";

interface ResizablePanelProps {
    children: ReactNode;
    isOpen: boolean;
    onClose: () => void;
    defaultWidth?: number;
    minWidth?: number;
    maxWidth?: number;
}

export default function ResizablePanel({
    children,
    isOpen,
    onClose,
    defaultWidth = 33,
    minWidth = 20,
    maxWidth = 60,
}: ResizablePanelProps) {
    const [width, setWidth] = useState(defaultWidth);
    const [isResizing, setIsResizing] = useState(false);
    const panelRef = useRef<HTMLDivElement>(null);

    useEffect(() => {
        const handleMouseMove = (e: MouseEvent) => {
            if (!isResizing || !panelRef.current) return;

            const containerWidth = panelRef.current.parentElement?.offsetWidth || 0;
            const newWidth = ((containerWidth - e.clientX) / containerWidth) * 100;

            if (newWidth >= minWidth && newWidth <= maxWidth) {
                setWidth(newWidth);
            }
        };

        const handleMouseUp = () => {
            setIsResizing(false);
        };

        if (isResizing) {
            document.addEventListener("mousemove", handleMouseMove);
            document.addEventListener("mouseup", handleMouseUp);
        }

        return () => {
            document.removeEventListener("mousemove", handleMouseMove);
            document.removeEventListener("mouseup", handleMouseUp);
        };
    }, [isResizing, minWidth, maxWidth]);

    if (!isOpen) return null;

    return (
        <>
            <div
                className="resizer"
                onMouseDown={() => setIsResizing(true)}
                style={{
                    width: "4px",
                    cursor: "col-resize",
                    background: isResizing ? "#4F8AD4" : "#2A476C",
                    transition: isResizing ? "none" : "background 0.2s",
                    flexShrink: 0,
                }}
            />
            <div
                ref={panelRef}
                style={{
                    width: `${width}%`,
                    display: "flex",
                    flexDirection: "column",
                    overflow: "hidden",
                    minHeight: 0,
                }}
            >
                {children}
            </div>
        </>
    );
}
