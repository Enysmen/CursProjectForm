export interface ICreatorTheme {
    themeName?: string;
    iconsSet?: string;
    cssVariables?: {
        [index: string]: string;
    };
}
export declare const PredefinedCreatorThemes: string[];
export declare const CreatorThemes: {
    [index: string]: ICreatorTheme;
};
