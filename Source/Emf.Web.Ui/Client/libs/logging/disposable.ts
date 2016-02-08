
import _ = require("lodash");

export interface IDisposable {
    dispose(): void;
}

export class CompositeDisposable implements IDisposable {

    private _disposables: IDisposable[] = [];

    add(disposable: IDisposable) {
        this._disposables.push(disposable);
        return this;
    }

    dispose(): void {
        _(this._disposables).reverse().forEach(d => d.dispose());
    }
}

export class Disposable implements IDisposable {

    private _disposed = false;

    constructor(private _dispose: () => void) {
    }

    dispose(): void {
        if (this._disposed)
            return;
        this._disposed = true;
        this._dispose();
    }
}

export class SerialDisposable implements IDisposable {

    private _disposable: IDisposable;
    private _disposed = false;

    setDisposable(disposable: IDisposable) {
        if (this._disposed) {
            disposable.dispose();
        } else {
            if (this._disposable)
                this._disposable.dispose();

            this._disposable = disposable;
        }
    }

    dispose(): void {
        this._disposed = true;
        if (this._disposable) {
            this._disposable.dispose();
            this._disposable = null;
        }
    }
}