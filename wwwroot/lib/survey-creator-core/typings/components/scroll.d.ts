export declare class ScrollViewModel {
    private _containerElementValue;
    private _rootElementValue;
    private _scrollbarElement;
    private _containerBodyElement;
    private _scrollbarSizerElement;
    private _containerBodyResizeObserver;
    private _lockScroll;
    constructor();
    setRootElement(element: HTMLElement): void;
    onScrollContainer(): void;
    onScrollScrollbar(): void;
    unsubscribeRootElement(): void;
}
