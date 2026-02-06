import { createDarkTheme, BrandVariants } from "@fluentui/react-components";

const dataverseBrand: BrandVariants = {
    10: "#020305",
    20: "#0F1419",
    30: "#16212D",
    40: "#1B2A3D",
    50: "#20334C",
    60: "#253D5C",
    70: "#2A476C",
    80: "#2F527D",
    90: "#355D8E",
    100: "#3B689F",
    110: "#4173B0",
    120: "#487EC2",
    130: "#4F8AD4",
    140: "#5795E3",
    150: "#69A1E8",
    160: "#7CAEED",
};

export const dataverseDarkTheme = createDarkTheme(dataverseBrand);

dataverseDarkTheme.colorNeutralBackground1 = "#0F1419";
dataverseDarkTheme.colorNeutralBackground2 = "#16212D";
dataverseDarkTheme.colorNeutralBackground3 = "#1B2A3D";
dataverseDarkTheme.colorNeutralForeground1 = "#E8EAED";
dataverseDarkTheme.colorNeutralForeground2 = "#BDC1C6";
dataverseDarkTheme.colorNeutralForeground3 = "#9AA0A6";
