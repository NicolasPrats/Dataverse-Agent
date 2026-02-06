export interface AgentConfig {
    displayName: string;
    avatar: string;
    color: string;
}

export const agentConfigs: Record<string, AgentConfig> = {
    "DataModelBuilder": {
        displayName: "Data Model Builder",
        avatar: "/PP/data.png",
        color: "#4F8AD4"
    },
    "UIBuilder": {
        displayName: "UIBuilder",
        avatar: "/PP/forms.png",
        color: "#7CAEED"
    },
    "Architect": {
        displayName: "Architect",
        avatar: "/PP/architect.png",
        color: "#4F8AD4"
    },
    "default": {
        displayName: "Unknown Agent",
        avatar: "/PP/human.png",
        color: "#9AA0A6"
    }
};

export function getAgentConfig(agentName: string): AgentConfig {
    console.log(`Retrieving config for agent: ${agentName}`);
    return agentConfigs[agentName] || agentConfigs["default"];
}
