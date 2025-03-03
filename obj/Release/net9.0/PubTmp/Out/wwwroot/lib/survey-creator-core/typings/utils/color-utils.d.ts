export declare class ColorCalculator {
    colorSettings: {
        baseColorAlpha: number;
        darkColorAlpha: number;
        lightColorAlpha: number;
        deltaDarkColor: number;
        deltaLightColor: number;
        newColorLight: string;
        newColorDark: string;
    };
    initialize(baseColor: string, lightColor: string, darkColor: string): void;
    calculateColors(newColor: string): void;
}
export declare function ingectAlpha(baseColor: any, alpha: number): any;
export declare function convertRgbaToString(rgbValues: Array<number>, alpha: number): string;
export declare function parseRgbaFromString(value?: string): Array<number>;
export declare function parseColor(value?: string): {
    color: string;
    opacity: number;
};
export declare function HEXToRGB(baseColor: any): any;
export declare function HSBToRGB(h: any, s: any, b: any): number[];
export declare function RGBToHSB(r: any, g: any, b: any): number[];
export declare function colorsAreEqual(color1: string, color2: string): boolean;
