declare function acquireVsCodeApi(): {
    postMessage(message: unknown): void;
    getState<T = unknown>(): T | undefined;
    setState<T>(state: T): void;
};

let _api: ReturnType<typeof acquireVsCodeApi> | undefined;

export function getVsCodeApi() {
    if (!_api) {
        _api = acquireVsCodeApi();
    }
    return _api;
}
