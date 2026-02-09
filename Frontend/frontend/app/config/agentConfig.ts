export interface AgentConfig {
    displayName: string;
    avatar: string;
    color: string;
}

export const agentConfigs: Record<string, AgentConfig> = {
    "DataModelBuilder": {
        displayName: "Data Model Builder Robot",
        avatar: "/PP/data.png",
        color: "#4F8AD4"
    },
    "UIBuilder": {
        displayName: "UI Builder Robot",
        avatar: "/PP/forms.png",
        color: "#7CAEED"
    },
    "Architect": {
        displayName: "Architect Robot",
        avatar: "/PP/architect.png",
        color: "#4F8AD4"
    },
    "Handyman": {
        displayName: "Handyman Robot",
        avatar: "/PP/handyman.png",
        color: "#4F8AD4"
    },
    "default": {
        displayName: "Unknown Robot",
        avatar: "/PP/unknown.png",
        color: "#9AA0A6"
    }
};

export function getAgentConfig(agentName: string): AgentConfig {
    console.log(`Retrieving config for agent: ${agentName}`);
    return agentConfigs[agentName] || agentConfigs["default"];
}
