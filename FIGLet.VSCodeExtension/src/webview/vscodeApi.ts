/**
 * VS Code webview API surface injected by the extension host at runtime.
 *
 * `acquireVsCodeApi` is a globally available function inside every VS Code
 * webview. It may only be called **once** per webview lifetime; subsequent
 * calls throw. Use {@link getVsCodeApi} instead, which caches the result.
 */
declare function acquireVsCodeApi(): {
    postMessage(message: unknown): void;
    getState<T = unknown>(): T | undefined;
    setState<T>(state: T): void;
};

/** Cached singleton returned by `acquireVsCodeApi`; `undefined` before first access. */
let _api: ReturnType<typeof acquireVsCodeApi> | undefined;

/**
 * Returns the VS Code webview API object, acquiring it on the first call and
 * returning the cached instance on subsequent calls.
 *
 * Because `acquireVsCodeApi` may only be invoked once per webview lifetime,
 * all webview code should obtain the API exclusively through this function.
 *
 * @returns The VS Code webview API instance.
 */
export function getVsCodeApi() {
    if (!_api) {
        _api = acquireVsCodeApi();
    }
    return _api;
}
