import { Base, EventBase, ILoadFromJSONOptions, ISaveToJSONOptions } from "survey-core";
import { ICreatorTheme } from "./creator-themes";
export declare class CreatorThemeModel extends Base implements ICreatorTheme {
    static defautlThemeName: string;
    initialCssVariables: {
        [index: string]: string;
    };
    themeCssVariablesChanges?: {
        [index: string]: string;
    };
    private primaryColorCalculator;
    private secondaryColorCalculator;
    private backgroundColorCalculator;
    unitDictionary: {
        [index: string]: number;
    };
    themeName: string;
    scale: number;
    fontScale: number;
    isLight: boolean;
    onThemeSelected: EventBase<CreatorThemeModel, {
        theme: ICreatorTheme;
    }>;
    onThemePropertyChanged: EventBase<CreatorThemeModel, {
        name: string;
        value: any;
    }>;
    private initializeColorCalculators;
    private initializeColorCalculator;
    private updateColorPropertiesDependentOnBaseColor;
    private isSpecialBackgroundFromCurrentTheme;
    private findAppropriateSpecialBackground;
    private updateBackgroundColor;
    constructor();
    getType(): string;
    get cssVariables(): {
        [index: string]: string;
    };
    private cssVariableChangedHandler;
    private setThemeCssVariablesChanges;
    private resetColorThemeCssVariablesChanges;
    private onThemePropertyValueChangedCallback;
    private scalePropertiesChanged;
    private scalingProperties;
    private scaleValue;
    private scaleCssVariables;
    private getScaleFactor;
    private updateScaleProperties;
    private blockThemeChangedNotifications;
    loadTheme(theme?: ICreatorTheme): void;
    setTheme(theme: ICreatorTheme): void;
    fromJSON(json: ICreatorTheme, options?: ILoadFromJSONOptions): void;
    toJSON(options?: ISaveToJSONOptions): ICreatorTheme;
}
