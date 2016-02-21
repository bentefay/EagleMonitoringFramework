export interface IStatefulErrorHandler {
    showError(message: string);
    clearError();
}