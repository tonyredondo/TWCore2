(window["webpackJsonp"] = window["webpackJsonp"] || []).push([["views-diagnostics-diagnostics-module"],{

/***/ "./node_modules/@nomadreservations/ngx-codemirror/fesm5/nomadreservations-ngx-codemirror.js":
/*!**************************************************************************************************!*\
  !*** ./node_modules/@nomadreservations/ngx-codemirror/fesm5/nomadreservations-ngx-codemirror.js ***!
  \**************************************************************************************************/
/*! exports provided: CodemirrorComponent, CodemirrorModule, CodemirrorService */
/***/ (function(module, __webpack_exports__, __webpack_require__) {

"use strict";
__webpack_require__.r(__webpack_exports__);
/* harmony export (binding) */ __webpack_require__.d(__webpack_exports__, "CodemirrorComponent", function() { return CodemirrorComponent; });
/* harmony export (binding) */ __webpack_require__.d(__webpack_exports__, "CodemirrorModule", function() { return CodemirrorModule; });
/* harmony export (binding) */ __webpack_require__.d(__webpack_exports__, "CodemirrorService", function() { return CodemirrorService; });
/* harmony import */ var _angular_core__WEBPACK_IMPORTED_MODULE_0__ = __webpack_require__(/*! @angular/core */ "./node_modules/@angular/core/fesm5/core.js");
/* harmony import */ var rxjs__WEBPACK_IMPORTED_MODULE_1__ = __webpack_require__(/*! rxjs */ "./node_modules/rxjs/_esm5/index.js");
/* harmony import */ var _angular_forms__WEBPACK_IMPORTED_MODULE_2__ = __webpack_require__(/*! @angular/forms */ "./node_modules/@angular/forms/fesm5/forms.js");




/**
 * @fileoverview added by tsickle
 * @suppress {checkTypes} checked by tsc
 */
/**
 * Main Codemirror import, utilizing window's existence to determine if we're server side or not.
 */
var /** @type {?} */ CodeMirror = typeof window !== 'undefined' && typeof window.navigator !== 'undefined'
    ? __webpack_require__(/*! codemirror */ "./node_modules/codemirror/lib/codemirror.js")
    : undefined;
/**
 * Initialize Event for CodeMirror.Editor instance
 *
 * Holds a referencable pointer to the code mirror instance for users.
 */
var CodemirrorService = /** @class */ (function () {
    function CodemirrorService() {
        /**
         * Codemirror instance subject
         *
         * Emits a refrence to the initialized CodeMirror.Editor once it's insantiated.
         */
        this.instance$ = new rxjs__WEBPACK_IMPORTED_MODULE_1__["ReplaySubject"]();
    }
    CodemirrorService.decorators = [
        { type: _angular_core__WEBPACK_IMPORTED_MODULE_0__["Injectable"] },
    ];
    return CodemirrorService;
}());

/**
 * @fileoverview added by tsickle
 * @suppress {checkTypes} checked by tsc
 */
/**
 * Main Codemirror import, utilizing window's existence to determine if we're server side or not.
 */
var /** @type {?} */ CodeMirror$1 = typeof window !== 'undefined' && typeof window.navigator !== 'undefined'
    ? __webpack_require__(/*! codemirror */ "./node_modules/codemirror/lib/codemirror.js")
    : undefined;
/**
 * CodeMirror component
 *
 * **Usage** :
 * ```html
 *   <ngx-codemirror [(ngModel)]="data" [config]="{...}" (init)="onInit" (blur)="onBlur" (focus)="onFocus" ...></ngx-codemirror>
 * ```
 */
var CodemirrorComponent = /** @class */ (function () {
    /**
     * Constructor
     *
     * @param _zone NgZone injected for Initialization
     */
    function CodemirrorComponent(_codeMirror, _zone) {
        this._codeMirror = _codeMirror;
        this._zone = _zone;
        /**
         * change output event, pass through from codemirror
         */
        this.change = new _angular_core__WEBPACK_IMPORTED_MODULE_0__["EventEmitter"]();
        /**
         * focus output event, pass through from codemirror
         */
        this.focus = new _angular_core__WEBPACK_IMPORTED_MODULE_0__["EventEmitter"]();
        /**
         * blur output event, pass through from codemirror
         */
        this.blur = new _angular_core__WEBPACK_IMPORTED_MODULE_0__["EventEmitter"]();
        /**
         * cursorActivity output event, pass through from codemirror
         */
        this.cursorActivity = new _angular_core__WEBPACK_IMPORTED_MODULE_0__["EventEmitter"]();
        /**
         * Value storage
         */
        this._value = '';
    }
    Object.defineProperty(CodemirrorComponent.prototype, "value", {
        /** Implements ControlValueAccessor.value */
        get: /**
         * Implements ControlValueAccessor.value
         * @return {?}
         */
        function () { return this._value; },
        set: /**
         * Implements ControlValueAccessor.value
         * @param {?} v
         * @return {?}
         */
        function (v) {
            if (v !== this._value) {
                this._value = v;
                this.onChange(v);
            }
        },
        enumerable: true,
        configurable: true
    });
    /**
     * On component destroy
     * @return {?}
     */
    CodemirrorComponent.prototype.ngOnDestroy = /**
     * On component destroy
     * @return {?}
     */
    function () {
    };
    /**
     * On component view init
     * @return {?}
     */
    CodemirrorComponent.prototype.ngAfterViewInit = /**
     * On component view init
     * @return {?}
     */
    function () {
        this.config = this.config || {};
        this.codemirrorInit(this.config);
    };
    /**
     * Value update process
     * @param {?} value
     * @return {?}
     */
    CodemirrorComponent.prototype.updateValue = /**
     * Value update process
     * @param {?} value
     * @return {?}
     */
    function (value) {
        this.value = value;
        this.onTouched();
        this.change.emit(value);
    };
    /**
     * Implements ControlValueAccessor
     * @param {?} value
     * @return {?}
     */
    CodemirrorComponent.prototype.writeValue = /**
     * Implements ControlValueAccessor
     * @param {?} value
     * @return {?}
     */
    function (value) {
        this._value = value || '';
        if (this._instance) {
            this._instance.setValue(this._value);
        }
    };
    /**
     * Change event trigger
     * @param {?} _
     * @return {?}
     */
    CodemirrorComponent.prototype.onChange = /**
     * Change event trigger
     * @param {?} _
     * @return {?}
     */
    function (_) { };
    /**
     * Dirty/touched event trigger
     * @return {?}
     */
    CodemirrorComponent.prototype.onTouched = /**
     * Dirty/touched event trigger
     * @return {?}
     */
    function () { };
    /**
     * Implements ControlValueAccessor.registerOnChange
     * @param {?} fn
     * @return {?}
     */
    CodemirrorComponent.prototype.registerOnChange = /**
     * Implements ControlValueAccessor.registerOnChange
     * @param {?} fn
     * @return {?}
     */
    function (fn) { this.onChange = fn; };
    /**
     * Implements ControlValueAccessor.registerOnTouched
     * @param {?} fn
     * @return {?}
     */
    CodemirrorComponent.prototype.registerOnTouched = /**
     * Implements ControlValueAccessor.registerOnTouched
     * @param {?} fn
     * @return {?}
     */
    function (fn) { this.onTouched = fn; };
    /**
     * Initialize codemirror
     * @param {?} config
     * @return {?}
     */
    CodemirrorComponent.prototype.codemirrorInit = /**
     * Initialize codemirror
     * @param {?} config
     * @return {?}
     */
    function (config) {
        var _this = this;
        if (CodeMirror$1) {
            this._zone.runOutsideAngular(function () {
                _this._instance = CodeMirror$1.fromTextArea(_this.host.nativeElement, config);
                _this._instance.setValue(_this._value);
            });
            this._instance.on('change', function () {
                _this.updateValue(_this._instance.getValue());
            });
            this._instance.on('focus', function (instance, event) {
                _this.focus.emit({ instance: instance, event: event });
            });
            this._instance.on('cursorActivity', function (instance) {
                _this.cursorActivity.emit({ instance: instance });
            });
            this._instance.on('blur', function (instance, event) {
                _this.blur.emit({ instance: instance, event: event });
            });
            this._codeMirror.instance$.next(this._instance);
        }
    };
    CodemirrorComponent.decorators = [
        { type: _angular_core__WEBPACK_IMPORTED_MODULE_0__["Component"], args: [{
                    // tslint:disable-next-line:component-selector
                    selector: 'ngx-codemirror',
                    providers: [
                        {
                            provide: _angular_forms__WEBPACK_IMPORTED_MODULE_2__["NG_VALUE_ACCESSOR"],
                            // tslint:disable-next-line:no-forward-ref
                            useExisting: Object(_angular_core__WEBPACK_IMPORTED_MODULE_0__["forwardRef"])(function () { return CodemirrorComponent; }),
                            multi: true
                        }
                    ],
                    template: '<textarea #host></textarea>',
                },] },
    ];
    /** @nocollapse */
    CodemirrorComponent.ctorParameters = function () { return [
        { type: CodemirrorService, },
        { type: _angular_core__WEBPACK_IMPORTED_MODULE_0__["NgZone"], },
    ]; };
    CodemirrorComponent.propDecorators = {
        "config": [{ type: _angular_core__WEBPACK_IMPORTED_MODULE_0__["Input"] },],
        "change": [{ type: _angular_core__WEBPACK_IMPORTED_MODULE_0__["Output"] },],
        "focus": [{ type: _angular_core__WEBPACK_IMPORTED_MODULE_0__["Output"] },],
        "blur": [{ type: _angular_core__WEBPACK_IMPORTED_MODULE_0__["Output"] },],
        "cursorActivity": [{ type: _angular_core__WEBPACK_IMPORTED_MODULE_0__["Output"] },],
        "host": [{ type: _angular_core__WEBPACK_IMPORTED_MODULE_0__["ViewChild"], args: ['host',] },],
        "value": [{ type: _angular_core__WEBPACK_IMPORTED_MODULE_0__["Input"] },],
    };
    return CodemirrorComponent;
}());

/**
 * @fileoverview added by tsickle
 * @suppress {checkTypes} checked by tsc
 */
/**
 * \@angular 5.x+ SSR ready CodeMirror wrapping module.
 */
var CodemirrorModule = /** @class */ (function () {
    function CodemirrorModule() {
    }
    CodemirrorModule.decorators = [
        { type: _angular_core__WEBPACK_IMPORTED_MODULE_0__["NgModule"], args: [{
                    providers: [
                        CodemirrorService
                    ],
                    declarations: [CodemirrorComponent],
                    exports: [CodemirrorComponent],
                    entryComponents: [CodemirrorComponent]
                },] },
    ];
    return CodemirrorModule;
}());

/**
 * @fileoverview added by tsickle
 * @suppress {checkTypes} checked by tsc
 */

/**
 * @fileoverview added by tsickle
 * @suppress {checkTypes} checked by tsc
 */

/**
 * @fileoverview added by tsickle
 * @suppress {checkTypes} checked by tsc
 */



//# sourceMappingURL=data:application/json;charset=utf-8;base64,eyJ2ZXJzaW9uIjozLCJmaWxlIjoibm9tYWRyZXNlcnZhdGlvbnMtbmd4LWNvZGVtaXJyb3IuanMubWFwIiwic291cmNlcyI6WyJuZzovL0Bub21hZHJlc2VydmF0aW9ucy9uZ3gtY29kZW1pcnJvci9uZ3gtY29kZW1pcnJvci9jb2RlbWlycm9yLnNlcnZpY2UudHMiLCJuZzovL0Bub21hZHJlc2VydmF0aW9ucy9uZ3gtY29kZW1pcnJvci9uZ3gtY29kZW1pcnJvci9jb2RlbWlycm9yLmNvbXBvbmVudC50cyIsIm5nOi8vQG5vbWFkcmVzZXJ2YXRpb25zL25neC1jb2RlbWlycm9yL25neC1jb2RlbWlycm9yL2NvZGVtaXJyb3IubW9kdWxlLnRzIl0sInNvdXJjZXNDb250ZW50IjpbIlxuaW1wb3J0IHsgRWRpdG9yIH0gZnJvbSAnY29kZW1pcnJvcic7XG5pbXBvcnQgeyBJbmplY3RhYmxlIH0gZnJvbSAnQGFuZ3VsYXIvY29yZSc7XG5pbXBvcnQgeyBSZXBsYXlTdWJqZWN0IH0gZnJvbSAncnhqcyc7XG5cbi8qKlxuICogTWFpbiBDb2RlbWlycm9yIGltcG9ydCwgdXRpbGl6aW5nIHdpbmRvdydzIGV4aXN0ZW5jZSB0byBkZXRlcm1pbmUgaWYgd2UncmUgc2VydmVyIHNpZGUgb3Igbm90LlxuICovXG4vLyB0c2xpbnQ6ZGlzYWJsZS1uZXh0LWxpbmU6dmFyaWFibGUtbmFtZVxuY29uc3QgQ29kZU1pcnJvcjogYW55ID1cbiAgdHlwZW9mIHdpbmRvdyAhPT0gJ3VuZGVmaW5lZCcgJiYgdHlwZW9mIHdpbmRvdy5uYXZpZ2F0b3IgIT09ICd1bmRlZmluZWQnXG4gID8gcmVxdWlyZSgnY29kZW1pcnJvcicpXG4gIDogdW5kZWZpbmVkO1xuXG4vKipcbiAqIEluaXRpYWxpemUgRXZlbnQgZm9yIENvZGVNaXJyb3IuRWRpdG9yIGluc3RhbmNlXG4gKlxuICogSG9sZHMgYSByZWZlcmVuY2FibGUgcG9pbnRlciB0byB0aGUgY29kZSBtaXJyb3IgaW5zdGFuY2UgZm9yIHVzZXJzLlxuICovXG5ASW5qZWN0YWJsZSgpXG5leHBvcnQgY2xhc3MgQ29kZW1pcnJvclNlcnZpY2Uge1xuICAvKipcbiAgICogQ29kZW1pcnJvciBpbnN0YW5jZSBzdWJqZWN0XG4gICAqXG4gICAqIEVtaXRzIGEgcmVmcmVuY2UgdG8gdGhlIGluaXRpYWxpemVkIENvZGVNaXJyb3IuRWRpdG9yIG9uY2UgaXQncyBpbnNhbnRpYXRlZC5cbiAgICovXG4gIHB1YmxpYyBpbnN0YW5jZSQ6IFJlcGxheVN1YmplY3Q8RWRpdG9yPiA9IG5ldyBSZXBsYXlTdWJqZWN0PEVkaXRvcj4oKTtcbn1cbiIsIi8vIEltcG9ydHNcbmltcG9ydCB7XG4gIENvbXBvbmVudCxcbiAgSW5wdXQsXG4gIE91dHB1dCxcbiAgRWxlbWVudFJlZixcbiAgVmlld0NoaWxkLFxuICBFdmVudEVtaXR0ZXIsXG4gIGZvcndhcmRSZWYsXG4gIEFmdGVyVmlld0luaXQsXG4gIE9uRGVzdHJveSxcbiAgTmdab25lLFxufSBmcm9tICdAYW5ndWxhci9jb3JlJztcbmltcG9ydCB7IE5HX1ZBTFVFX0FDQ0VTU09SIH0gZnJvbSAnQGFuZ3VsYXIvZm9ybXMnO1xuaW1wb3J0IHsgUmVwbGF5U3ViamVjdCB9IGZyb20gJ3J4anMnO1xuXG5pbXBvcnQgeyBFZGl0b3IgfSBmcm9tICdjb2RlbWlycm9yJztcbmltcG9ydCB7IENvZGVtaXJyb3JTZXJ2aWNlIH0gZnJvbSAnLi9jb2RlbWlycm9yLnNlcnZpY2UnO1xuXG4vKipcbiAqIE1haW4gQ29kZW1pcnJvciBpbXBvcnQsIHV0aWxpemluZyB3aW5kb3cncyBleGlzdGVuY2UgdG8gZGV0ZXJtaW5lIGlmIHdlJ3JlIHNlcnZlciBzaWRlIG9yIG5vdC5cbiAqL1xuLy8gdHNsaW50OmRpc2FibGUtbmV4dC1saW5lOnZhcmlhYmxlLW5hbWVcbmNvbnN0IENvZGVNaXJyb3I6IGFueSA9XG4gIHR5cGVvZiB3aW5kb3cgIT09ICd1bmRlZmluZWQnICYmIHR5cGVvZiB3aW5kb3cubmF2aWdhdG9yICE9PSAndW5kZWZpbmVkJ1xuICA/IHJlcXVpcmUoJ2NvZGVtaXJyb3InKVxuICA6IHVuZGVmaW5lZDtcblxuLyoqXG4gKiBDb2RlTWlycm9yIGNvbXBvbmVudFxuICpcbiAqICoqVXNhZ2UqKiA6XG4gKiBgYGBodG1sXG4gKiAgIDxuZ3gtY29kZW1pcnJvciBbKG5nTW9kZWwpXT1cImRhdGFcIiBbY29uZmlnXT1cInsuLi59XCIgKGluaXQpPVwib25Jbml0XCIgKGJsdXIpPVwib25CbHVyXCIgKGZvY3VzKT1cIm9uRm9jdXNcIiAuLi4+PC9uZ3gtY29kZW1pcnJvcj5cbiAqIGBgYFxuICovXG5AQ29tcG9uZW50KHtcbiAgLy8gdHNsaW50OmRpc2FibGUtbmV4dC1saW5lOmNvbXBvbmVudC1zZWxlY3RvclxuICBzZWxlY3RvcjogJ25neC1jb2RlbWlycm9yJyxcbiAgcHJvdmlkZXJzOiBbXG4gICAge1xuICAgICAgcHJvdmlkZTogTkdfVkFMVUVfQUNDRVNTT1IsXG4gICAgICAvLyB0c2xpbnQ6ZGlzYWJsZS1uZXh0LWxpbmU6bm8tZm9yd2FyZC1yZWZcbiAgICAgIHVzZUV4aXN0aW5nOiBmb3J3YXJkUmVmKCgpID0+IENvZGVtaXJyb3JDb21wb25lbnQpLFxuICAgICAgbXVsdGk6IHRydWVcbiAgICB9XG4gIF0sXG4gIHRlbXBsYXRlOiAnPHRleHRhcmVhICNob3N0PjwvdGV4dGFyZWE+Jyxcbn0pXG5leHBvcnQgY2xhc3MgQ29kZW1pcnJvckNvbXBvbmVudCBpbXBsZW1lbnRzIEFmdGVyVmlld0luaXQsIE9uRGVzdHJveSB7XG5cbiAgLyoqIENvZGVtaXJyb3IgY29uZmlnIG9iamVjdCAoc2VlIFtkZXRhaWxzXShodHRwOi8vY29kZW1pcnJvci5uZXQvZG9jL21hbnVhbC5odG1sI2NvbmZpZykpICovXG4gIEBJbnB1dCgpIHB1YmxpYyBjb25maWc6IGFueTtcbiAgLyoqIGNoYW5nZSBvdXRwdXQgZXZlbnQsIHBhc3MgdGhyb3VnaCBmcm9tIGNvZGVtaXJyb3IgKi9cbiAgQE91dHB1dCgpIHB1YmxpYyBjaGFuZ2UgPSBuZXcgRXZlbnRFbWl0dGVyKCk7XG4gIC8qKiBmb2N1cyBvdXRwdXQgZXZlbnQsIHBhc3MgdGhyb3VnaCBmcm9tIGNvZGVtaXJyb3IgKi9cbiAgQE91dHB1dCgpIHB1YmxpYyBmb2N1cyA9IG5ldyBFdmVudEVtaXR0ZXIoKTtcbiAgLyoqIGJsdXIgb3V0cHV0IGV2ZW50LCBwYXNzIHRocm91Z2ggZnJvbSBjb2RlbWlycm9yICovXG4gIEBPdXRwdXQoKSBwdWJsaWMgYmx1ciA9IG5ldyBFdmVudEVtaXR0ZXIoKTtcbiAgLyoqIGN1cnNvckFjdGl2aXR5IG91dHB1dCBldmVudCwgcGFzcyB0aHJvdWdoIGZyb20gY29kZW1pcnJvciAqL1xuICBAT3V0cHV0KCkgcHVibGljIGN1cnNvckFjdGl2aXR5ID0gbmV3IEV2ZW50RW1pdHRlcigpO1xuICAvKiogSG9zdCBlbGVtZW50IGZvciBjb2RlbWlycm9yIHRvIGF0dGFjaCB0byAqL1xuICBAVmlld0NoaWxkKCdob3N0JykgcHVibGljIGhvc3Q6IEVsZW1lbnRSZWY7XG5cbiAgLyoqIEN1cnJlbnQgZWRpdG9yIGluc3RhbmNlICovXG4gIHByaXZhdGUgX2luc3RhbmNlOiBhbnk7XG5cbiAgLyoqIFZhbHVlIHN0b3JhZ2UgKi9cbiAgcHJpdmF0ZSBfdmFsdWUgPSAnJztcblxuICAvKipcbiAgICogQ29uc3RydWN0b3JcbiAgICpcbiAgICogQHBhcmFtIF96b25lIE5nWm9uZSBpbmplY3RlZCBmb3IgSW5pdGlhbGl6YXRpb25cbiAgICovXG4gIGNvbnN0cnVjdG9yKFxuICAgIHByaXZhdGUgX2NvZGVNaXJyb3I6IENvZGVtaXJyb3JTZXJ2aWNlLFxuICAgIHByaXZhdGUgX3pvbmU6IE5nWm9uZVxuICApIHt9XG5cbiAgLyoqIEltcGxlbWVudHMgQ29udHJvbFZhbHVlQWNjZXNzb3IudmFsdWUgKi9cbiAgZ2V0IHZhbHVlKCkgeyByZXR1cm4gdGhpcy5fdmFsdWU7IH1cblxuICAvKiogSW1wbGVtZW50cyBDb250cm9sVmFsdWVBY2Nlc3Nvci52YWx1ZSAqL1xuICBASW5wdXQoKSBzZXQgdmFsdWUodikge1xuICAgIGlmICh2ICE9PSB0aGlzLl92YWx1ZSkge1xuICAgICAgdGhpcy5fdmFsdWUgPSB2O1xuICAgICAgdGhpcy5vbkNoYW5nZSh2KTtcbiAgICB9XG4gIH1cblxuICAvKipcbiAgICogT24gY29tcG9uZW50IGRlc3Ryb3lcbiAgICovXG4gIHB1YmxpYyBuZ09uRGVzdHJveSgpIHtcblxuICB9XG5cbiAgLyoqXG4gICAqIE9uIGNvbXBvbmVudCB2aWV3IGluaXRcbiAgICovXG4gIHB1YmxpYyBuZ0FmdGVyVmlld0luaXQoKSB7XG4gICAgdGhpcy5jb25maWcgPSB0aGlzLmNvbmZpZyB8fCB7fTtcbiAgICB0aGlzLmNvZGVtaXJyb3JJbml0KHRoaXMuY29uZmlnKTtcbiAgfVxuXG4gIC8qKlxuICAgKiBWYWx1ZSB1cGRhdGUgcHJvY2Vzc1xuICAgKi9cbiAgcHVibGljIHVwZGF0ZVZhbHVlKHZhbHVlOiBhbnkpIHtcbiAgICB0aGlzLnZhbHVlID0gdmFsdWU7XG4gICAgdGhpcy5vblRvdWNoZWQoKTtcbiAgICB0aGlzLmNoYW5nZS5lbWl0KHZhbHVlKTtcbiAgfVxuXG4gIC8qKlxuICAgKiBJbXBsZW1lbnRzIENvbnRyb2xWYWx1ZUFjY2Vzc29yXG4gICAqL1xuICBwdWJsaWMgd3JpdGVWYWx1ZSh2YWx1ZTogYW55KSB7XG4gICAgdGhpcy5fdmFsdWUgPSB2YWx1ZSB8fCAnJztcbiAgICBpZiAodGhpcy5faW5zdGFuY2UpIHtcbiAgICAgIHRoaXMuX2luc3RhbmNlLnNldFZhbHVlKHRoaXMuX3ZhbHVlKTtcbiAgICB9XG4gIH1cblxuICAvKiogQ2hhbmdlIGV2ZW50IHRyaWdnZXIgKi9cbiAgcHVibGljICBvbkNoYW5nZShfOiBhbnkpIHt9XG4gIC8qKiBEaXJ0eS90b3VjaGVkIGV2ZW50IHRyaWdnZXIgKi9cbiAgcHVibGljIG9uVG91Y2hlZCgpIHt9XG4gIC8qKiBJbXBsZW1lbnRzIENvbnRyb2xWYWx1ZUFjY2Vzc29yLnJlZ2lzdGVyT25DaGFuZ2UgKi9cbiAgcHVibGljIHJlZ2lzdGVyT25DaGFuZ2UoZm46IGFueSkgeyB0aGlzLm9uQ2hhbmdlID0gZm47IH1cbiAgLyoqIEltcGxlbWVudHMgQ29udHJvbFZhbHVlQWNjZXNzb3IucmVnaXN0ZXJPblRvdWNoZWQgKi9cbiAgcHVibGljIHJlZ2lzdGVyT25Ub3VjaGVkKGZuOiBhbnkpIHsgdGhpcy5vblRvdWNoZWQgPSBmbjsgfVxuXG4gIC8qKlxuICAgKiBJbml0aWFsaXplIGNvZGVtaXJyb3JcbiAgICovXG4gIHByaXZhdGUgY29kZW1pcnJvckluaXQoY29uZmlnOiBhbnkpIHtcbiAgICBpZiAoQ29kZU1pcnJvcikge1xuICAgICAgdGhpcy5fem9uZS5ydW5PdXRzaWRlQW5ndWxhcigoKSA9PiB7XG4gICAgICAgIHRoaXMuX2luc3RhbmNlID0gQ29kZU1pcnJvci5mcm9tVGV4dEFyZWEodGhpcy5ob3N0Lm5hdGl2ZUVsZW1lbnQsIGNvbmZpZyk7XG4gICAgICAgIHRoaXMuX2luc3RhbmNlLnNldFZhbHVlKHRoaXMuX3ZhbHVlKTtcbiAgICAgIH0pO1xuXG4gICAgICB0aGlzLl9pbnN0YW5jZS5vbignY2hhbmdlJywgKCkgPT4ge1xuICAgICAgICB0aGlzLnVwZGF0ZVZhbHVlKHRoaXMuX2luc3RhbmNlLmdldFZhbHVlKCkpO1xuICAgICAgfSk7XG5cbiAgICAgIHRoaXMuX2luc3RhbmNlLm9uKCdmb2N1cycsIChpbnN0YW5jZTogYW55LCBldmVudDogYW55KSA9PiB7XG4gICAgICAgIHRoaXMuZm9jdXMuZW1pdCh7aW5zdGFuY2UsIGV2ZW50fSk7XG4gICAgICB9KTtcblxuICAgICAgdGhpcy5faW5zdGFuY2Uub24oJ2N1cnNvckFjdGl2aXR5JywgKGluc3RhbmNlOiBhbnkpID0+IHtcbiAgICAgICAgdGhpcy5jdXJzb3JBY3Rpdml0eS5lbWl0KHtpbnN0YW5jZX0pO1xuICAgICAgfSk7XG5cbiAgICAgIHRoaXMuX2luc3RhbmNlLm9uKCdibHVyJywgKGluc3RhbmNlOiBhbnksIGV2ZW50OiBhbnkpID0+IHtcbiAgICAgICAgdGhpcy5ibHVyLmVtaXQoe2luc3RhbmNlLCBldmVudH0pO1xuICAgICAgfSk7XG5cbiAgICAgIHRoaXMuX2NvZGVNaXJyb3IuaW5zdGFuY2UkLm5leHQodGhpcy5faW5zdGFuY2UpO1xuICAgIH1cbiAgfVxufVxuIiwiaW1wb3J0IHsgTmdNb2R1bGUgfSBmcm9tICdAYW5ndWxhci9jb3JlJztcblxuaW1wb3J0IHsgQ29kZW1pcnJvckNvbXBvbmVudCB9IGZyb20gJy4vY29kZW1pcnJvci5jb21wb25lbnQnO1xuaW1wb3J0IHsgQ29kZW1pcnJvclNlcnZpY2UgfSBmcm9tICcuL2NvZGVtaXJyb3Iuc2VydmljZSc7XG5cbi8qKlxuICogQGFuZ3VsYXIgNS54KyBTU1IgcmVhZHkgQ29kZU1pcnJvciB3cmFwcGluZyBtb2R1bGUuXG4gKi9cbkBOZ01vZHVsZSh7XG4gIHByb3ZpZGVyczogW1xuICAgIENvZGVtaXJyb3JTZXJ2aWNlXG4gIF0sXG4gIGRlY2xhcmF0aW9uczogW0NvZGVtaXJyb3JDb21wb25lbnRdLFxuICBleHBvcnRzOiBbQ29kZW1pcnJvckNvbXBvbmVudF0sXG4gIGVudHJ5Q29tcG9uZW50czogW0NvZGVtaXJyb3JDb21wb25lbnRdXG59KVxuZXhwb3J0IGNsYXNzIENvZGVtaXJyb3JNb2R1bGUge31cbiJdLCJuYW1lcyI6WyJDb2RlTWlycm9yIl0sIm1hcHBpbmdzIjoiOzs7Ozs7OztBQUVBOzs7QUFPQSxxQkFBTSxVQUFVLEdBQ2QsT0FBTyxNQUFNLEtBQUssV0FBVyxJQUFJLE9BQU8sTUFBTSxDQUFDLFNBQVMsS0FBSyxXQUFXO01BQ3RFLE9BQU8sQ0FBQyxZQUFZLENBQUM7TUFDckIsU0FBUyxDQUFDOzs7Ozs7Ozs7Ozs7O3lCQWM4QixJQUFJLGFBQWEsRUFBVTs7O2dCQVB0RSxVQUFVOzs0QkFuQlg7Ozs7Ozs7QUNDQTs7O0FBc0JBLHFCQUFNQSxZQUFVLEdBQ2QsT0FBTyxNQUFNLEtBQUssV0FBVyxJQUFJLE9BQU8sTUFBTSxDQUFDLFNBQVMsS0FBSyxXQUFXO01BQ3RFLE9BQU8sQ0FBQyxZQUFZLENBQUM7TUFDckIsU0FBUyxDQUFDOzs7Ozs7Ozs7Ozs7Ozs7SUFpRFosNkJBQ1UsYUFDQTtRQURBLGdCQUFXLEdBQVgsV0FBVztRQUNYLFVBQUssR0FBTCxLQUFLOzs7O3NCQXZCVyxJQUFJLFlBQVksRUFBRTs7OztxQkFFbkIsSUFBSSxZQUFZLEVBQUU7Ozs7b0JBRW5CLElBQUksWUFBWSxFQUFFOzs7OzhCQUVSLElBQUksWUFBWSxFQUFFOzs7O3NCQVFuQyxFQUFFO0tBVWY7SUFHSixzQkFBSSxzQ0FBSzs7Ozs7O1FBQVQsY0FBYyxPQUFPLElBQUksQ0FBQyxNQUFNLENBQUMsRUFBRTs7Ozs7O2tCQUdoQixDQUFDO1lBQ2xCLElBQUksQ0FBQyxLQUFLLElBQUksQ0FBQyxNQUFNLEVBQUU7Z0JBQ3JCLElBQUksQ0FBQyxNQUFNLEdBQUcsQ0FBQyxDQUFDO2dCQUNoQixJQUFJLENBQUMsUUFBUSxDQUFDLENBQUMsQ0FBQyxDQUFDO2FBQ2xCOzs7O09BUGdDOzs7OztJQWE1Qix5Q0FBVzs7Ozs7Ozs7OztJQU9YLDZDQUFlOzs7OztRQUNwQixJQUFJLENBQUMsTUFBTSxHQUFHLElBQUksQ0FBQyxNQUFNLElBQUksRUFBRSxDQUFDO1FBQ2hDLElBQUksQ0FBQyxjQUFjLENBQUMsSUFBSSxDQUFDLE1BQU0sQ0FBQyxDQUFDOzs7Ozs7O0lBTTVCLHlDQUFXOzs7OztjQUFDLEtBQVU7UUFDM0IsSUFBSSxDQUFDLEtBQUssR0FBRyxLQUFLLENBQUM7UUFDbkIsSUFBSSxDQUFDLFNBQVMsRUFBRSxDQUFDO1FBQ2pCLElBQUksQ0FBQyxNQUFNLENBQUMsSUFBSSxDQUFDLEtBQUssQ0FBQyxDQUFDOzs7Ozs7O0lBTW5CLHdDQUFVOzs7OztjQUFDLEtBQVU7UUFDMUIsSUFBSSxDQUFDLE1BQU0sR0FBRyxLQUFLLElBQUksRUFBRSxDQUFDO1FBQzFCLElBQUksSUFBSSxDQUFDLFNBQVMsRUFBRTtZQUNsQixJQUFJLENBQUMsU0FBUyxDQUFDLFFBQVEsQ0FBQyxJQUFJLENBQUMsTUFBTSxDQUFDLENBQUM7U0FDdEM7Ozs7Ozs7SUFJSyxzQ0FBUTs7Ozs7Y0FBQyxDQUFNOzs7OztJQUVoQix1Q0FBUzs7Ozs7Ozs7OztJQUVULDhDQUFnQjs7Ozs7Y0FBQyxFQUFPLElBQUksSUFBSSxDQUFDLFFBQVEsR0FBRyxFQUFFLENBQUM7Ozs7OztJQUUvQywrQ0FBaUI7Ozs7O2NBQUMsRUFBTyxJQUFJLElBQUksQ0FBQyxTQUFTLEdBQUcsRUFBRSxDQUFDOzs7Ozs7SUFLaEQsNENBQWM7Ozs7O2NBQUMsTUFBVzs7UUFDaEMsSUFBSUEsWUFBVSxFQUFFO1lBQ2QsSUFBSSxDQUFDLEtBQUssQ0FBQyxpQkFBaUIsQ0FBQztnQkFDM0IsS0FBSSxDQUFDLFNBQVMsR0FBR0EsWUFBVSxDQUFDLFlBQVksQ0FBQyxLQUFJLENBQUMsSUFBSSxDQUFDLGFBQWEsRUFBRSxNQUFNLENBQUMsQ0FBQztnQkFDMUUsS0FBSSxDQUFDLFNBQVMsQ0FBQyxRQUFRLENBQUMsS0FBSSxDQUFDLE1BQU0sQ0FBQyxDQUFDO2FBQ3RDLENBQUMsQ0FBQztZQUVILElBQUksQ0FBQyxTQUFTLENBQUMsRUFBRSxDQUFDLFFBQVEsRUFBRTtnQkFDMUIsS0FBSSxDQUFDLFdBQVcsQ0FBQyxLQUFJLENBQUMsU0FBUyxDQUFDLFFBQVEsRUFBRSxDQUFDLENBQUM7YUFDN0MsQ0FBQyxDQUFDO1lBRUgsSUFBSSxDQUFDLFNBQVMsQ0FBQyxFQUFFLENBQUMsT0FBTyxFQUFFLFVBQUMsUUFBYSxFQUFFLEtBQVU7Z0JBQ25ELEtBQUksQ0FBQyxLQUFLLENBQUMsSUFBSSxDQUFDLEVBQUMsUUFBUSxVQUFBLEVBQUUsS0FBSyxPQUFBLEVBQUMsQ0FBQyxDQUFDO2FBQ3BDLENBQUMsQ0FBQztZQUVILElBQUksQ0FBQyxTQUFTLENBQUMsRUFBRSxDQUFDLGdCQUFnQixFQUFFLFVBQUMsUUFBYTtnQkFDaEQsS0FBSSxDQUFDLGNBQWMsQ0FBQyxJQUFJLENBQUMsRUFBQyxRQUFRLFVBQUEsRUFBQyxDQUFDLENBQUM7YUFDdEMsQ0FBQyxDQUFDO1lBRUgsSUFBSSxDQUFDLFNBQVMsQ0FBQyxFQUFFLENBQUMsTUFBTSxFQUFFLFVBQUMsUUFBYSxFQUFFLEtBQVU7Z0JBQ2xELEtBQUksQ0FBQyxJQUFJLENBQUMsSUFBSSxDQUFDLEVBQUMsUUFBUSxVQUFBLEVBQUUsS0FBSyxPQUFBLEVBQUMsQ0FBQyxDQUFDO2FBQ25DLENBQUMsQ0FBQztZQUVILElBQUksQ0FBQyxXQUFXLENBQUMsU0FBUyxDQUFDLElBQUksQ0FBQyxJQUFJLENBQUMsU0FBUyxDQUFDLENBQUM7U0FDakQ7OztnQkE3SEosU0FBUyxTQUFDOztvQkFFVCxRQUFRLEVBQUUsZ0JBQWdCO29CQUMxQixTQUFTLEVBQUU7d0JBQ1Q7NEJBQ0UsT0FBTyxFQUFFLGlCQUFpQjs7NEJBRTFCLFdBQVcsRUFBRSxVQUFVLENBQUMsY0FBTSxPQUFBLG1CQUFtQixHQUFBLENBQUM7NEJBQ2xELEtBQUssRUFBRSxJQUFJO3lCQUNaO3FCQUNGO29CQUNELFFBQVEsRUFBRSw2QkFBNkI7aUJBQ3hDOzs7O2dCQS9CUSxpQkFBaUI7Z0JBTnhCLE1BQU07OzsyQkF5Q0wsS0FBSzsyQkFFTCxNQUFNOzBCQUVOLE1BQU07eUJBRU4sTUFBTTttQ0FFTixNQUFNO3lCQUVOLFNBQVMsU0FBQyxNQUFNOzBCQXNCaEIsS0FBSzs7OEJBcEZSOzs7Ozs7O0FDQUE7Ozs7Ozs7Z0JBUUMsUUFBUSxTQUFDO29CQUNSLFNBQVMsRUFBRTt3QkFDVCxpQkFBaUI7cUJBQ2xCO29CQUNELFlBQVksRUFBRSxDQUFDLG1CQUFtQixDQUFDO29CQUNuQyxPQUFPLEVBQUUsQ0FBQyxtQkFBbUIsQ0FBQztvQkFDOUIsZUFBZSxFQUFFLENBQUMsbUJBQW1CLENBQUM7aUJBQ3ZDOzsyQkFmRDs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7In0=

/***/ }),

/***/ "./node_modules/ngx-bootstrap/chronos/create/check-overflow.js":
/*!*********************************************************************!*\
  !*** ./node_modules/ngx-bootstrap/chronos/create/check-overflow.js ***!
  \*********************************************************************/
/*! exports provided: checkOverflow */
/***/ (function(module, __webpack_exports__, __webpack_require__) {

"use strict";
__webpack_require__.r(__webpack_exports__);
/* harmony export (binding) */ __webpack_require__.d(__webpack_exports__, "checkOverflow", function() { return checkOverflow; });
/* harmony import */ var _parsing_flags__WEBPACK_IMPORTED_MODULE_0__ = __webpack_require__(/*! ./parsing-flags */ "./node_modules/ngx-bootstrap/chronos/create/parsing-flags.js");
/* harmony import */ var _units_constants__WEBPACK_IMPORTED_MODULE_1__ = __webpack_require__(/*! ../units/constants */ "./node_modules/ngx-bootstrap/chronos/units/constants.js");
/* harmony import */ var _units_month__WEBPACK_IMPORTED_MODULE_2__ = __webpack_require__(/*! ../units/month */ "./node_modules/ngx-bootstrap/chronos/units/month.js");



function checkOverflow(config) {
    var overflow;
    var a = config._a;
    if (a && Object(_parsing_flags__WEBPACK_IMPORTED_MODULE_0__["getParsingFlags"])(config).overflow === -2) {
        // todo: fix this sh*t
        overflow =
            a[_units_constants__WEBPACK_IMPORTED_MODULE_1__["MONTH"]] < 0 || a[_units_constants__WEBPACK_IMPORTED_MODULE_1__["MONTH"]] > 11 ? _units_constants__WEBPACK_IMPORTED_MODULE_1__["MONTH"] :
                a[_units_constants__WEBPACK_IMPORTED_MODULE_1__["DATE"]] < 1 || a[_units_constants__WEBPACK_IMPORTED_MODULE_1__["DATE"]] > Object(_units_month__WEBPACK_IMPORTED_MODULE_2__["daysInMonth"])(a[_units_constants__WEBPACK_IMPORTED_MODULE_1__["YEAR"]], a[_units_constants__WEBPACK_IMPORTED_MODULE_1__["MONTH"]]) ? _units_constants__WEBPACK_IMPORTED_MODULE_1__["DATE"] :
                    a[_units_constants__WEBPACK_IMPORTED_MODULE_1__["HOUR"]] < 0 || a[_units_constants__WEBPACK_IMPORTED_MODULE_1__["HOUR"]] > 24 || (a[_units_constants__WEBPACK_IMPORTED_MODULE_1__["HOUR"]] === 24 && (a[_units_constants__WEBPACK_IMPORTED_MODULE_1__["MINUTE"]] !== 0 || a[_units_constants__WEBPACK_IMPORTED_MODULE_1__["SECOND"]] !== 0 || a[_units_constants__WEBPACK_IMPORTED_MODULE_1__["MILLISECOND"]] !== 0)) ? _units_constants__WEBPACK_IMPORTED_MODULE_1__["HOUR"] :
                        a[_units_constants__WEBPACK_IMPORTED_MODULE_1__["MINUTE"]] < 0 || a[_units_constants__WEBPACK_IMPORTED_MODULE_1__["MINUTE"]] > 59 ? _units_constants__WEBPACK_IMPORTED_MODULE_1__["MINUTE"] :
                            a[_units_constants__WEBPACK_IMPORTED_MODULE_1__["SECOND"]] < 0 || a[_units_constants__WEBPACK_IMPORTED_MODULE_1__["SECOND"]] > 59 ? _units_constants__WEBPACK_IMPORTED_MODULE_1__["SECOND"] :
                                a[_units_constants__WEBPACK_IMPORTED_MODULE_1__["MILLISECOND"]] < 0 || a[_units_constants__WEBPACK_IMPORTED_MODULE_1__["MILLISECOND"]] > 999 ? _units_constants__WEBPACK_IMPORTED_MODULE_1__["MILLISECOND"] :
                                    -1;
        if (Object(_parsing_flags__WEBPACK_IMPORTED_MODULE_0__["getParsingFlags"])(config)._overflowDayOfYear && (overflow < _units_constants__WEBPACK_IMPORTED_MODULE_1__["YEAR"] || overflow > _units_constants__WEBPACK_IMPORTED_MODULE_1__["DATE"])) {
            overflow = _units_constants__WEBPACK_IMPORTED_MODULE_1__["DATE"];
        }
        if (Object(_parsing_flags__WEBPACK_IMPORTED_MODULE_0__["getParsingFlags"])(config)._overflowWeeks && overflow === -1) {
            overflow = _units_constants__WEBPACK_IMPORTED_MODULE_1__["WEEK"];
        }
        if (Object(_parsing_flags__WEBPACK_IMPORTED_MODULE_0__["getParsingFlags"])(config)._overflowWeekday && overflow === -1) {
            overflow = _units_constants__WEBPACK_IMPORTED_MODULE_1__["WEEKDAY"];
        }
        Object(_parsing_flags__WEBPACK_IMPORTED_MODULE_0__["getParsingFlags"])(config).overflow = overflow;
    }
    return config;
}
//# sourceMappingURL=check-overflow.js.map

/***/ }),

/***/ "./node_modules/ngx-bootstrap/chronos/create/clone.js":
/*!************************************************************!*\
  !*** ./node_modules/ngx-bootstrap/chronos/create/clone.js ***!
  \************************************************************/
/*! exports provided: cloneDate */
/***/ (function(module, __webpack_exports__, __webpack_require__) {

"use strict";
__webpack_require__.r(__webpack_exports__);
/* harmony export (binding) */ __webpack_require__.d(__webpack_exports__, "cloneDate", function() { return cloneDate; });
// fastest way to clone date
// https://jsperf.com/clone-date-object2
function cloneDate(date) {
    return new Date(date.getTime());
}
//# sourceMappingURL=clone.js.map

/***/ }),

/***/ "./node_modules/ngx-bootstrap/chronos/create/date-from-array.js":
/*!**********************************************************************!*\
  !*** ./node_modules/ngx-bootstrap/chronos/create/date-from-array.js ***!
  \**********************************************************************/
/*! exports provided: createUTCDate, createDate */
/***/ (function(module, __webpack_exports__, __webpack_require__) {

"use strict";
__webpack_require__.r(__webpack_exports__);
/* harmony export (binding) */ __webpack_require__.d(__webpack_exports__, "createUTCDate", function() { return createUTCDate; });
/* harmony export (binding) */ __webpack_require__.d(__webpack_exports__, "createDate", function() { return createDate; });
function createUTCDate(y, m, d) {
    var date = new Date(Date.UTC.apply(null, arguments));
    // the Date.UTC function remaps years 0-99 to 1900-1999
    if (y < 100 && y >= 0 && isFinite(date.getUTCFullYear())) {
        date.setUTCFullYear(y);
    }
    return date;
}
function createDate(y, m, d, h, M, s, ms) {
    if (m === void 0) { m = 0; }
    if (d === void 0) { d = 1; }
    if (h === void 0) { h = 0; }
    if (M === void 0) { M = 0; }
    if (s === void 0) { s = 0; }
    if (ms === void 0) { ms = 0; }
    var date = new Date(y, m, d, h, M, s, ms);
    // the date constructor remaps years 0-99 to 1900-1999
    if (y < 100 && y >= 0 && isFinite(date.getFullYear())) {
        date.setFullYear(y);
    }
    return date;
}
//# sourceMappingURL=date-from-array.js.map

/***/ }),

/***/ "./node_modules/ngx-bootstrap/chronos/create/from-anything.js":
/*!********************************************************************!*\
  !*** ./node_modules/ngx-bootstrap/chronos/create/from-anything.js ***!
  \********************************************************************/
/*! exports provided: prepareConfig, createLocalOrUTC */
/***/ (function(module, __webpack_exports__, __webpack_require__) {

"use strict";
__webpack_require__.r(__webpack_exports__);
/* harmony export (binding) */ __webpack_require__.d(__webpack_exports__, "prepareConfig", function() { return prepareConfig; });
/* harmony export (binding) */ __webpack_require__.d(__webpack_exports__, "createLocalOrUTC", function() { return createLocalOrUTC; });
/* harmony import */ var _utils_type_checks__WEBPACK_IMPORTED_MODULE_0__ = __webpack_require__(/*! ../utils/type-checks */ "./node_modules/ngx-bootstrap/chronos/utils/type-checks.js");
/* harmony import */ var _locale_locales__WEBPACK_IMPORTED_MODULE_1__ = __webpack_require__(/*! ../locale/locales */ "./node_modules/ngx-bootstrap/chronos/locale/locales.js");
/* harmony import */ var _valid__WEBPACK_IMPORTED_MODULE_2__ = __webpack_require__(/*! ./valid */ "./node_modules/ngx-bootstrap/chronos/create/valid.js");
/* harmony import */ var _from_string_and_array__WEBPACK_IMPORTED_MODULE_3__ = __webpack_require__(/*! ./from-string-and-array */ "./node_modules/ngx-bootstrap/chronos/create/from-string-and-array.js");
/* harmony import */ var _from_string_and_format__WEBPACK_IMPORTED_MODULE_4__ = __webpack_require__(/*! ./from-string-and-format */ "./node_modules/ngx-bootstrap/chronos/create/from-string-and-format.js");
/* harmony import */ var _clone__WEBPACK_IMPORTED_MODULE_5__ = __webpack_require__(/*! ./clone */ "./node_modules/ngx-bootstrap/chronos/create/clone.js");
/* harmony import */ var _from_string__WEBPACK_IMPORTED_MODULE_6__ = __webpack_require__(/*! ./from-string */ "./node_modules/ngx-bootstrap/chronos/create/from-string.js");
/* harmony import */ var _from_array__WEBPACK_IMPORTED_MODULE_7__ = __webpack_require__(/*! ./from-array */ "./node_modules/ngx-bootstrap/chronos/create/from-array.js");
/* harmony import */ var _from_object__WEBPACK_IMPORTED_MODULE_8__ = __webpack_require__(/*! ./from-object */ "./node_modules/ngx-bootstrap/chronos/create/from-object.js");
/* harmony import */ var _check_overflow__WEBPACK_IMPORTED_MODULE_9__ = __webpack_require__(/*! ./check-overflow */ "./node_modules/ngx-bootstrap/chronos/create/check-overflow.js");










function createFromConfig(config) {
    var res = Object(_check_overflow__WEBPACK_IMPORTED_MODULE_9__["checkOverflow"])(prepareConfig(config));
    // todo: remove, in moment.js it's never called cuz of moment constructor
    res._d = new Date(res._d != null ? res._d.getTime() : NaN);
    if (!Object(_valid__WEBPACK_IMPORTED_MODULE_2__["isValid"])(Object.assign({}, res, { _isValid: null }))) {
        res._d = new Date(NaN);
    }
    // todo: update offset
    /*if (res._nextDay) {
        // Adding is smart enough around DST
        res._d = add(res._d, 1, 'day');
        res._nextDay = undefined;
      }*/
    return res;
}
function prepareConfig(config) {
    var input = config._i;
    var format = config._f;
    config._locale = config._locale || Object(_locale_locales__WEBPACK_IMPORTED_MODULE_1__["getLocale"])(config._l);
    if (input === null || (format === undefined && input === '')) {
        return Object(_valid__WEBPACK_IMPORTED_MODULE_2__["createInvalid"])(config, { nullInput: true });
    }
    if (Object(_utils_type_checks__WEBPACK_IMPORTED_MODULE_0__["isString"])(input)) {
        config._i = input = config._locale.preparse(input);
    }
    if (Object(_utils_type_checks__WEBPACK_IMPORTED_MODULE_0__["isDate"])(input)) {
        config._d = Object(_clone__WEBPACK_IMPORTED_MODULE_5__["cloneDate"])(input);
        return config;
    }
    // todo: add check for recursion
    if (Object(_utils_type_checks__WEBPACK_IMPORTED_MODULE_0__["isArray"])(format)) {
        Object(_from_string_and_array__WEBPACK_IMPORTED_MODULE_3__["configFromStringAndArray"])(config);
    }
    else if (format) {
        Object(_from_string_and_format__WEBPACK_IMPORTED_MODULE_4__["configFromStringAndFormat"])(config);
    }
    else {
        configFromInput(config);
    }
    if (!Object(_valid__WEBPACK_IMPORTED_MODULE_2__["isValid"])(config)) {
        config._d = null;
    }
    return config;
}
function configFromInput(config) {
    var input = config._i;
    if (Object(_utils_type_checks__WEBPACK_IMPORTED_MODULE_0__["isUndefined"])(input)) {
        config._d = new Date();
    }
    else if (Object(_utils_type_checks__WEBPACK_IMPORTED_MODULE_0__["isDate"])(input)) {
        config._d = Object(_clone__WEBPACK_IMPORTED_MODULE_5__["cloneDate"])(input);
    }
    else if (Object(_utils_type_checks__WEBPACK_IMPORTED_MODULE_0__["isString"])(input)) {
        Object(_from_string__WEBPACK_IMPORTED_MODULE_6__["configFromString"])(config);
    }
    else if (Object(_utils_type_checks__WEBPACK_IMPORTED_MODULE_0__["isArray"])(input) && input.length) {
        var _arr = input.slice(0);
        config._a = _arr.map(function (obj) { return Object(_utils_type_checks__WEBPACK_IMPORTED_MODULE_0__["isString"])(obj) ? parseInt(obj, 10) : obj; });
        Object(_from_array__WEBPACK_IMPORTED_MODULE_7__["configFromArray"])(config);
    }
    else if (Object(_utils_type_checks__WEBPACK_IMPORTED_MODULE_0__["isObject"])(input)) {
        Object(_from_object__WEBPACK_IMPORTED_MODULE_8__["configFromObject"])(config);
    }
    else if (Object(_utils_type_checks__WEBPACK_IMPORTED_MODULE_0__["isNumber"])(input)) {
        // from milliseconds
        config._d = new Date(input);
    }
    else {
        //   hooks.createFromInputFallback(config);
        return Object(_valid__WEBPACK_IMPORTED_MODULE_2__["createInvalid"])(config);
    }
    return config;
}
function createLocalOrUTC(input, format, localeKey, strict, isUTC) {
    var config = {};
    var _input = input;
    // params switch -> skip; test it well
    // if (localeKey === true || localeKey === false) {
    //     strict = localeKey;
    //     localeKey = undefined;
    // }
    // todo: fail fast and return not valid date
    if ((Object(_utils_type_checks__WEBPACK_IMPORTED_MODULE_0__["isObject"])(_input) && Object(_utils_type_checks__WEBPACK_IMPORTED_MODULE_0__["isObjectEmpty"])(_input)) || (Object(_utils_type_checks__WEBPACK_IMPORTED_MODULE_0__["isArray"])(_input) && _input.length === 0)) {
        _input = undefined;
    }
    // object construction must be done this way.
    // https://github.com/moment/moment/issues/1423
    // config._isAMomentObject = true;
    config._useUTC = config._isUTC = isUTC;
    config._l = localeKey;
    config._i = _input;
    config._f = format;
    config._strict = strict;
    return createFromConfig(config);
}
//# sourceMappingURL=from-anything.js.map

/***/ }),

/***/ "./node_modules/ngx-bootstrap/chronos/create/from-array.js":
/*!*****************************************************************!*\
  !*** ./node_modules/ngx-bootstrap/chronos/create/from-array.js ***!
  \*****************************************************************/
/*! exports provided: configFromArray */
/***/ (function(module, __webpack_exports__, __webpack_require__) {

"use strict";
__webpack_require__.r(__webpack_exports__);
/* harmony export (binding) */ __webpack_require__.d(__webpack_exports__, "configFromArray", function() { return configFromArray; });
/* harmony import */ var _units_constants__WEBPACK_IMPORTED_MODULE_0__ = __webpack_require__(/*! ../units/constants */ "./node_modules/ngx-bootstrap/chronos/units/constants.js");
/* harmony import */ var _units_year__WEBPACK_IMPORTED_MODULE_1__ = __webpack_require__(/*! ../units/year */ "./node_modules/ngx-bootstrap/chronos/units/year.js");
/* harmony import */ var _parsing_flags__WEBPACK_IMPORTED_MODULE_2__ = __webpack_require__(/*! ./parsing-flags */ "./node_modules/ngx-bootstrap/chronos/create/parsing-flags.js");
/* harmony import */ var _date_from_array__WEBPACK_IMPORTED_MODULE_3__ = __webpack_require__(/*! ./date-from-array */ "./node_modules/ngx-bootstrap/chronos/create/date-from-array.js");
/* harmony import */ var _units_week_calendar_utils__WEBPACK_IMPORTED_MODULE_4__ = __webpack_require__(/*! ../units/week-calendar-utils */ "./node_modules/ngx-bootstrap/chronos/units/week-calendar-utils.js");
/* harmony import */ var _utils_defaults__WEBPACK_IMPORTED_MODULE_5__ = __webpack_require__(/*! ../utils/defaults */ "./node_modules/ngx-bootstrap/chronos/utils/defaults.js");







function currentDateArray(config) {
    var nowValue = new Date();
    if (config._useUTC) {
        return [nowValue.getUTCFullYear(), nowValue.getUTCMonth(), nowValue.getUTCDate()];
    }
    return [nowValue.getFullYear(), nowValue.getMonth(), nowValue.getDate()];
}
// convert an array to a date.
// the array should mirror the parameters below
// note: all values past the year are optional and will default to the lowest possible value.
// [year, month, day , hour, minute, second, millisecond]
function configFromArray(config) {
    var input = [];
    var i;
    var date;
    var currentDate;
    var expectedWeekday;
    var yearToUse;
    if (config._d) {
        return config;
    }
    currentDate = currentDateArray(config);
    // compute day of the year from weeks and weekdays
    if (config._w && config._a[_units_constants__WEBPACK_IMPORTED_MODULE_0__["DATE"]] == null && config._a[_units_constants__WEBPACK_IMPORTED_MODULE_0__["MONTH"]] == null) {
        dayOfYearFromWeekInfo(config);
    }
    // if the day of the year is set, figure out what it is
    if (config._dayOfYear != null) {
        yearToUse = Object(_utils_defaults__WEBPACK_IMPORTED_MODULE_5__["defaults"])(config._a[_units_constants__WEBPACK_IMPORTED_MODULE_0__["YEAR"]], currentDate[_units_constants__WEBPACK_IMPORTED_MODULE_0__["YEAR"]]);
        if (config._dayOfYear > Object(_units_year__WEBPACK_IMPORTED_MODULE_1__["daysInYear"])(yearToUse) || config._dayOfYear === 0) {
            Object(_parsing_flags__WEBPACK_IMPORTED_MODULE_2__["getParsingFlags"])(config)._overflowDayOfYear = true;
        }
        date = new Date(Date.UTC(yearToUse, 0, config._dayOfYear));
        config._a[_units_constants__WEBPACK_IMPORTED_MODULE_0__["MONTH"]] = date.getUTCMonth();
        config._a[_units_constants__WEBPACK_IMPORTED_MODULE_0__["DATE"]] = date.getUTCDate();
    }
    // Default to current date.
    // * if no year, month, day of month are given, default to today
    // * if day of month is given, default month and year
    // * if month is given, default only year
    // * if year is given, don't default anything
    for (i = 0; i < 3 && config._a[i] == null; ++i) {
        config._a[i] = input[i] = currentDate[i];
    }
    // Zero out whatever was not defaulted, including time
    for (; i < 7; i++) {
        config._a[i] = input[i] = (config._a[i] == null) ? (i === 2 ? 1 : 0) : config._a[i];
    }
    // Check for 24:00:00.000
    if (config._a[_units_constants__WEBPACK_IMPORTED_MODULE_0__["HOUR"]] === 24 &&
        config._a[_units_constants__WEBPACK_IMPORTED_MODULE_0__["MINUTE"]] === 0 &&
        config._a[_units_constants__WEBPACK_IMPORTED_MODULE_0__["SECOND"]] === 0 &&
        config._a[_units_constants__WEBPACK_IMPORTED_MODULE_0__["MILLISECOND"]] === 0) {
        config._nextDay = true;
        config._a[_units_constants__WEBPACK_IMPORTED_MODULE_0__["HOUR"]] = 0;
    }
    config._d = (config._useUTC ? _date_from_array__WEBPACK_IMPORTED_MODULE_3__["createUTCDate"] : _date_from_array__WEBPACK_IMPORTED_MODULE_3__["createDate"]).apply(null, input);
    expectedWeekday = config._useUTC ? config._d.getUTCDay() : config._d.getDay();
    // Apply timezone offset from input. The actual utcOffset can be changed
    // with parseZone.
    if (config._tzm != null) {
        config._d.setUTCMinutes(config._d.getUTCMinutes() - config._tzm);
    }
    if (config._nextDay) {
        config._a[_units_constants__WEBPACK_IMPORTED_MODULE_0__["HOUR"]] = 24;
    }
    // check for mismatching day of week
    if (config._w && typeof config._w.d !== 'undefined' && config._w.d !== expectedWeekday) {
        Object(_parsing_flags__WEBPACK_IMPORTED_MODULE_2__["getParsingFlags"])(config).weekdayMismatch = true;
    }
    return config;
}
function dayOfYearFromWeekInfo(config) {
    var w, weekYear, week, weekday, dow, doy, temp, weekdayOverflow;
    w = config._w;
    if (w.GG != null || w.W != null || w.E != null) {
        dow = 1;
        doy = 4;
        // TODO: We need to take the current isoWeekYear, but that depends on
        // how we interpret now (local, utc, fixed offset). So create
        // a now version of current config (take local/utc/offset flags, and
        // create now).
        weekYear = Object(_utils_defaults__WEBPACK_IMPORTED_MODULE_5__["defaults"])(w.GG, config._a[_units_constants__WEBPACK_IMPORTED_MODULE_0__["YEAR"]], Object(_units_week_calendar_utils__WEBPACK_IMPORTED_MODULE_4__["weekOfYear"])(new Date(), 1, 4).year);
        week = Object(_utils_defaults__WEBPACK_IMPORTED_MODULE_5__["defaults"])(w.W, 1);
        weekday = Object(_utils_defaults__WEBPACK_IMPORTED_MODULE_5__["defaults"])(w.E, 1);
        if (weekday < 1 || weekday > 7) {
            weekdayOverflow = true;
        }
    }
    else {
        dow = config._locale._week.dow;
        doy = config._locale._week.doy;
        var curWeek = Object(_units_week_calendar_utils__WEBPACK_IMPORTED_MODULE_4__["weekOfYear"])(new Date(), dow, doy);
        weekYear = Object(_utils_defaults__WEBPACK_IMPORTED_MODULE_5__["defaults"])(w.gg, config._a[_units_constants__WEBPACK_IMPORTED_MODULE_0__["YEAR"]], curWeek.year);
        // Default to current week.
        week = Object(_utils_defaults__WEBPACK_IMPORTED_MODULE_5__["defaults"])(w.w, curWeek.week);
        if (w.d != null) {
            // weekday -- low day numbers are considered next week
            weekday = w.d;
            if (weekday < 0 || weekday > 6) {
                weekdayOverflow = true;
            }
        }
        else if (w.e != null) {
            // local weekday -- counting starts from begining of week
            weekday = w.e + dow;
            if (w.e < 0 || w.e > 6) {
                weekdayOverflow = true;
            }
        }
        else {
            // default to begining of week
            weekday = dow;
        }
    }
    if (week < 1 || week > Object(_units_week_calendar_utils__WEBPACK_IMPORTED_MODULE_4__["weeksInYear"])(weekYear, dow, doy)) {
        Object(_parsing_flags__WEBPACK_IMPORTED_MODULE_2__["getParsingFlags"])(config)._overflowWeeks = true;
    }
    else if (weekdayOverflow != null) {
        Object(_parsing_flags__WEBPACK_IMPORTED_MODULE_2__["getParsingFlags"])(config)._overflowWeekday = true;
    }
    else {
        temp = Object(_units_week_calendar_utils__WEBPACK_IMPORTED_MODULE_4__["dayOfYearFromWeeks"])(weekYear, week, weekday, dow, doy);
        config._a[_units_constants__WEBPACK_IMPORTED_MODULE_0__["YEAR"]] = temp.year;
        config._dayOfYear = temp.dayOfYear;
    }
    return config;
}
//# sourceMappingURL=from-array.js.map

/***/ }),

/***/ "./node_modules/ngx-bootstrap/chronos/create/from-object.js":
/*!******************************************************************!*\
  !*** ./node_modules/ngx-bootstrap/chronos/create/from-object.js ***!
  \******************************************************************/
/*! exports provided: configFromObject */
/***/ (function(module, __webpack_exports__, __webpack_require__) {

"use strict";
__webpack_require__.r(__webpack_exports__);
/* harmony export (binding) */ __webpack_require__.d(__webpack_exports__, "configFromObject", function() { return configFromObject; });
/* harmony import */ var _units_aliases__WEBPACK_IMPORTED_MODULE_0__ = __webpack_require__(/*! ../units/aliases */ "./node_modules/ngx-bootstrap/chronos/units/aliases.js");
/* harmony import */ var _from_array__WEBPACK_IMPORTED_MODULE_1__ = __webpack_require__(/*! ./from-array */ "./node_modules/ngx-bootstrap/chronos/create/from-array.js");
/* harmony import */ var _utils_type_checks__WEBPACK_IMPORTED_MODULE_2__ = __webpack_require__(/*! ../utils/type-checks */ "./node_modules/ngx-bootstrap/chronos/utils/type-checks.js");



function configFromObject(config) {
    if (config._d) {
        return config;
    }
    var input = config._i;
    if (Object(_utils_type_checks__WEBPACK_IMPORTED_MODULE_2__["isObject"])(input)) {
        var i = Object(_units_aliases__WEBPACK_IMPORTED_MODULE_0__["normalizeObjectUnits"])(input);
        config._a = [i.year, i.month, i.day, i.hours, i.minutes, i.seconds, i.milliseconds]
            .map(function (obj) { return Object(_utils_type_checks__WEBPACK_IMPORTED_MODULE_2__["isString"])(obj) ? parseInt(obj, 10) : obj; });
    }
    return Object(_from_array__WEBPACK_IMPORTED_MODULE_1__["configFromArray"])(config);
}
//# sourceMappingURL=from-object.js.map

/***/ }),

/***/ "./node_modules/ngx-bootstrap/chronos/create/from-string-and-array.js":
/*!****************************************************************************!*\
  !*** ./node_modules/ngx-bootstrap/chronos/create/from-string-and-array.js ***!
  \****************************************************************************/
/*! exports provided: configFromStringAndArray */
/***/ (function(module, __webpack_exports__, __webpack_require__) {

"use strict";
__webpack_require__.r(__webpack_exports__);
/* harmony export (binding) */ __webpack_require__.d(__webpack_exports__, "configFromStringAndArray", function() { return configFromStringAndArray; });
/* harmony import */ var _valid__WEBPACK_IMPORTED_MODULE_0__ = __webpack_require__(/*! ./valid */ "./node_modules/ngx-bootstrap/chronos/create/valid.js");
/* harmony import */ var _parsing_flags__WEBPACK_IMPORTED_MODULE_1__ = __webpack_require__(/*! ./parsing-flags */ "./node_modules/ngx-bootstrap/chronos/create/parsing-flags.js");
/* harmony import */ var _from_string_and_format__WEBPACK_IMPORTED_MODULE_2__ = __webpack_require__(/*! ./from-string-and-format */ "./node_modules/ngx-bootstrap/chronos/create/from-string-and-format.js");



// date from string and array of format strings
function configFromStringAndArray(config) {
    var tempConfig;
    var bestMoment;
    var scoreToBeat;
    var currentScore;
    if (!config._f || config._f.length === 0) {
        Object(_parsing_flags__WEBPACK_IMPORTED_MODULE_1__["getParsingFlags"])(config).invalidFormat = true;
        return Object(_valid__WEBPACK_IMPORTED_MODULE_0__["createInvalid"])(config);
    }
    var i;
    for (i = 0; i < config._f.length; i++) {
        currentScore = 0;
        tempConfig = Object.assign({}, config);
        if (config._useUTC != null) {
            tempConfig._useUTC = config._useUTC;
        }
        tempConfig._f = config._f[i];
        Object(_from_string_and_format__WEBPACK_IMPORTED_MODULE_2__["configFromStringAndFormat"])(tempConfig);
        if (!Object(_valid__WEBPACK_IMPORTED_MODULE_0__["isValid"])(tempConfig)) {
            continue;
        }
        // if there is any input that was not parsed add a penalty for that format
        currentScore += Object(_parsing_flags__WEBPACK_IMPORTED_MODULE_1__["getParsingFlags"])(tempConfig).charsLeftOver;
        // or tokens
        currentScore += Object(_parsing_flags__WEBPACK_IMPORTED_MODULE_1__["getParsingFlags"])(tempConfig).unusedTokens.length * 10;
        Object(_parsing_flags__WEBPACK_IMPORTED_MODULE_1__["getParsingFlags"])(tempConfig).score = currentScore;
        if (scoreToBeat == null || currentScore < scoreToBeat) {
            scoreToBeat = currentScore;
            bestMoment = tempConfig;
        }
    }
    return Object.assign(config, bestMoment || tempConfig);
}
//# sourceMappingURL=from-string-and-array.js.map

/***/ }),

/***/ "./node_modules/ngx-bootstrap/chronos/create/from-string-and-format.js":
/*!*****************************************************************************!*\
  !*** ./node_modules/ngx-bootstrap/chronos/create/from-string-and-format.js ***!
  \*****************************************************************************/
/*! exports provided: ISO_8601, RFC_2822, configFromStringAndFormat */
/***/ (function(module, __webpack_exports__, __webpack_require__) {

"use strict";
__webpack_require__.r(__webpack_exports__);
/* harmony export (binding) */ __webpack_require__.d(__webpack_exports__, "ISO_8601", function() { return ISO_8601; });
/* harmony export (binding) */ __webpack_require__.d(__webpack_exports__, "RFC_2822", function() { return RFC_2822; });
/* harmony export (binding) */ __webpack_require__.d(__webpack_exports__, "configFromStringAndFormat", function() { return configFromStringAndFormat; });
/* harmony import */ var _from_string__WEBPACK_IMPORTED_MODULE_0__ = __webpack_require__(/*! ./from-string */ "./node_modules/ngx-bootstrap/chronos/create/from-string.js");
/* harmony import */ var _format__WEBPACK_IMPORTED_MODULE_1__ = __webpack_require__(/*! ../format */ "./node_modules/ngx-bootstrap/chronos/format.js");
/* harmony import */ var _format_format__WEBPACK_IMPORTED_MODULE_2__ = __webpack_require__(/*! ../format/format */ "./node_modules/ngx-bootstrap/chronos/format/format.js");
/* harmony import */ var _utils_type_checks__WEBPACK_IMPORTED_MODULE_3__ = __webpack_require__(/*! ../utils/type-checks */ "./node_modules/ngx-bootstrap/chronos/utils/type-checks.js");
/* harmony import */ var _parse_regex__WEBPACK_IMPORTED_MODULE_4__ = __webpack_require__(/*! ../parse/regex */ "./node_modules/ngx-bootstrap/chronos/parse/regex.js");
/* harmony import */ var _parse_token__WEBPACK_IMPORTED_MODULE_5__ = __webpack_require__(/*! ../parse/token */ "./node_modules/ngx-bootstrap/chronos/parse/token.js");
/* harmony import */ var _units_constants__WEBPACK_IMPORTED_MODULE_6__ = __webpack_require__(/*! ../units/constants */ "./node_modules/ngx-bootstrap/chronos/units/constants.js");
/* harmony import */ var _from_array__WEBPACK_IMPORTED_MODULE_7__ = __webpack_require__(/*! ./from-array */ "./node_modules/ngx-bootstrap/chronos/create/from-array.js");
/* harmony import */ var _parsing_flags__WEBPACK_IMPORTED_MODULE_8__ = __webpack_require__(/*! ./parsing-flags */ "./node_modules/ngx-bootstrap/chronos/create/parsing-flags.js");
/* harmony import */ var _check_overflow__WEBPACK_IMPORTED_MODULE_9__ = __webpack_require__(/*! ./check-overflow */ "./node_modules/ngx-bootstrap/chronos/create/check-overflow.js");










// constant that refers to the ISO standard
// hooks.ISO_8601 = function () {};
var ISO_8601 = 'ISO_8601';
// constant that refers to the RFC 2822 form
// hooks.RFC_2822 = function () {};
var RFC_2822 = 'RFC_2822';
// date from string and format string
function configFromStringAndFormat(config) {
    // TODO: Move this to another part of the creation flow to prevent circular deps
    if (config._f === ISO_8601) {
        return Object(_from_string__WEBPACK_IMPORTED_MODULE_0__["configFromISO"])(config);
    }
    if (config._f === RFC_2822) {
        return Object(_from_string__WEBPACK_IMPORTED_MODULE_0__["configFromRFC2822"])(config);
    }
    config._a = [];
    Object(_parsing_flags__WEBPACK_IMPORTED_MODULE_8__["getParsingFlags"])(config).empty = true;
    if (Object(_utils_type_checks__WEBPACK_IMPORTED_MODULE_3__["isArray"])(config._f) || (!config._i && config._i !== 0)) {
        return config;
    }
    // This array is used to make a Date, either with `new Date` or `Date.UTC`
    var input = config._i.toString();
    var totalParsedInputLength = 0;
    var inputLength = input.length;
    var tokens = Object(_format__WEBPACK_IMPORTED_MODULE_1__["expandFormat"])(config._f, config._locale).match(_format_format__WEBPACK_IMPORTED_MODULE_2__["formattingTokens"]) || [];
    var i;
    var token;
    var parsedInput;
    var skipped;
    for (i = 0; i < tokens.length; i++) {
        token = tokens[i];
        parsedInput = (input.match(Object(_parse_regex__WEBPACK_IMPORTED_MODULE_4__["getParseRegexForToken"])(token, config._locale)) || [])[0];
        if (parsedInput) {
            skipped = input.substr(0, input.indexOf(parsedInput));
            if (skipped.length > 0) {
                Object(_parsing_flags__WEBPACK_IMPORTED_MODULE_8__["getParsingFlags"])(config).unusedInput.push(skipped);
            }
            input = input.slice(input.indexOf(parsedInput) + parsedInput.length);
            totalParsedInputLength += parsedInput.length;
        }
        // don't parse if it's not a known token
        if (_format_format__WEBPACK_IMPORTED_MODULE_2__["formatTokenFunctions"][token]) {
            if (parsedInput) {
                Object(_parsing_flags__WEBPACK_IMPORTED_MODULE_8__["getParsingFlags"])(config).empty = false;
            }
            else {
                Object(_parsing_flags__WEBPACK_IMPORTED_MODULE_8__["getParsingFlags"])(config).unusedTokens.push(token);
            }
            Object(_parse_token__WEBPACK_IMPORTED_MODULE_5__["addTimeToArrayFromToken"])(token, parsedInput, config);
        }
        else if (config._strict && !parsedInput) {
            Object(_parsing_flags__WEBPACK_IMPORTED_MODULE_8__["getParsingFlags"])(config).unusedTokens.push(token);
        }
    }
    // add remaining unparsed input length to the string
    Object(_parsing_flags__WEBPACK_IMPORTED_MODULE_8__["getParsingFlags"])(config).charsLeftOver = inputLength - totalParsedInputLength;
    if (input.length > 0) {
        Object(_parsing_flags__WEBPACK_IMPORTED_MODULE_8__["getParsingFlags"])(config).unusedInput.push(input);
    }
    // clear _12h flag if hour is <= 12
    if (config._a[_units_constants__WEBPACK_IMPORTED_MODULE_6__["HOUR"]] <= 12 &&
        Object(_parsing_flags__WEBPACK_IMPORTED_MODULE_8__["getParsingFlags"])(config).bigHour === true &&
        config._a[_units_constants__WEBPACK_IMPORTED_MODULE_6__["HOUR"]] > 0) {
        Object(_parsing_flags__WEBPACK_IMPORTED_MODULE_8__["getParsingFlags"])(config).bigHour = void 0;
    }
    Object(_parsing_flags__WEBPACK_IMPORTED_MODULE_8__["getParsingFlags"])(config).parsedDateParts = config._a.slice(0);
    Object(_parsing_flags__WEBPACK_IMPORTED_MODULE_8__["getParsingFlags"])(config).meridiem = config._meridiem;
    // handle meridiem
    config._a[_units_constants__WEBPACK_IMPORTED_MODULE_6__["HOUR"]] = meridiemFixWrap(config._locale, config._a[_units_constants__WEBPACK_IMPORTED_MODULE_6__["HOUR"]], config._meridiem);
    Object(_from_array__WEBPACK_IMPORTED_MODULE_7__["configFromArray"])(config);
    return Object(_check_overflow__WEBPACK_IMPORTED_MODULE_9__["checkOverflow"])(config);
}
function meridiemFixWrap(locale, _hour, meridiem) {
    var hour = _hour;
    if (meridiem == null) {
        // nothing to do
        return hour;
    }
    if (locale.meridiemHour != null) {
        return locale.meridiemHour(hour, meridiem);
    }
    if (locale.isPM == null) {
        // this is not supposed to happen
        return hour;
    }
    // Fallback
    var isPm = locale.isPM(meridiem);
    if (isPm && hour < 12) {
        hour += 12;
    }
    if (!isPm && hour === 12) {
        hour = 0;
    }
    return hour;
}
//# sourceMappingURL=from-string-and-format.js.map

/***/ }),

/***/ "./node_modules/ngx-bootstrap/chronos/create/from-string.js":
/*!******************************************************************!*\
  !*** ./node_modules/ngx-bootstrap/chronos/create/from-string.js ***!
  \******************************************************************/
/*! exports provided: configFromISO, configFromRFC2822, configFromString */
/***/ (function(module, __webpack_exports__, __webpack_require__) {

"use strict";
__webpack_require__.r(__webpack_exports__);
/* harmony export (binding) */ __webpack_require__.d(__webpack_exports__, "configFromISO", function() { return configFromISO; });
/* harmony export (binding) */ __webpack_require__.d(__webpack_exports__, "configFromRFC2822", function() { return configFromRFC2822; });
/* harmony export (binding) */ __webpack_require__.d(__webpack_exports__, "configFromString", function() { return configFromString; });
/* harmony import */ var _locale_locale_class__WEBPACK_IMPORTED_MODULE_0__ = __webpack_require__(/*! ../locale/locale.class */ "./node_modules/ngx-bootstrap/chronos/locale/locale.class.js");
/* harmony import */ var _utils_type_checks__WEBPACK_IMPORTED_MODULE_1__ = __webpack_require__(/*! ../utils/type-checks */ "./node_modules/ngx-bootstrap/chronos/utils/type-checks.js");
/* harmony import */ var _from_string_and_format__WEBPACK_IMPORTED_MODULE_2__ = __webpack_require__(/*! ./from-string-and-format */ "./node_modules/ngx-bootstrap/chronos/create/from-string-and-format.js");
/* harmony import */ var _date_from_array__WEBPACK_IMPORTED_MODULE_3__ = __webpack_require__(/*! ./date-from-array */ "./node_modules/ngx-bootstrap/chronos/create/date-from-array.js");
/* harmony import */ var _valid__WEBPACK_IMPORTED_MODULE_4__ = __webpack_require__(/*! ./valid */ "./node_modules/ngx-bootstrap/chronos/create/valid.js");
/* harmony import */ var _parsing_flags__WEBPACK_IMPORTED_MODULE_5__ = __webpack_require__(/*! ./parsing-flags */ "./node_modules/ngx-bootstrap/chronos/create/parsing-flags.js");






// iso 8601 regex
// 0000-00-00 0000-W00 or 0000-W00-0 + T + 00 or 00:00 or 00:00:00 or 00:00:00.000 + +00:00 or +0000 or +00)
// tslint:disable-next-line
var extendedIsoRegex = /^\s*((?:[+-]\d{6}|\d{4})-(?:\d\d-\d\d|W\d\d-\d|W\d\d|\d\d\d|\d\d))(?:(T| )(\d\d(?::\d\d(?::\d\d(?:[.,]\d+)?)?)?)([\+\-]\d\d(?::?\d\d)?|\s*Z)?)?$/;
// tslint:disable-next-line
var basicIsoRegex = /^\s*((?:[+-]\d{6}|\d{4})(?:\d\d\d\d|W\d\d\d|W\d\d|\d\d\d|\d\d))(?:(T| )(\d\d(?:\d\d(?:\d\d(?:[.,]\d+)?)?)?)([\+\-]\d\d(?::?\d\d)?|\s*Z)?)?$/;
var tzRegex = /Z|[+-]\d\d(?::?\d\d)?/;
var isoDates = [
    ['YYYYYY-MM-DD', /[+-]\d{6}-\d\d-\d\d/, true],
    ['YYYY-MM-DD', /\d{4}-\d\d-\d\d/, true],
    ['GGGG-[W]WW-E', /\d{4}-W\d\d-\d/, true],
    ['GGGG-[W]WW', /\d{4}-W\d\d/, false],
    ['YYYY-DDD', /\d{4}-\d{3}/, true],
    ['YYYY-MM', /\d{4}-\d\d/, false],
    ['YYYYYYMMDD', /[+-]\d{10}/, true],
    ['YYYYMMDD', /\d{8}/, true],
    // YYYYMM is NOT allowed by the standard
    ['GGGG[W]WWE', /\d{4}W\d{3}/, true],
    ['GGGG[W]WW', /\d{4}W\d{2}/, false],
    ['YYYYDDD', /\d{7}/, true]
];
// iso time formats and regexes
var isoTimes = [
    ['HH:mm:ss.SSSS', /\d\d:\d\d:\d\d\.\d+/],
    ['HH:mm:ss,SSSS', /\d\d:\d\d:\d\d,\d+/],
    ['HH:mm:ss', /\d\d:\d\d:\d\d/],
    ['HH:mm', /\d\d:\d\d/],
    ['HHmmss.SSSS', /\d\d\d\d\d\d\.\d+/],
    ['HHmmss,SSSS', /\d\d\d\d\d\d,\d+/],
    ['HHmmss', /\d\d\d\d\d\d/],
    ['HHmm', /\d\d\d\d/],
    ['HH', /\d\d/]
];
var aspNetJsonRegex = /^\/?Date\((\-?\d+)/i;
var obsOffsets = {
    UT: 0,
    GMT: 0,
    EDT: -4 * 60,
    EST: -5 * 60,
    CDT: -5 * 60,
    CST: -6 * 60,
    MDT: -6 * 60,
    MST: -7 * 60,
    PDT: -7 * 60,
    PST: -8 * 60
};
// RFC 2822 regex: For details see https://tools.ietf.org/html/rfc2822#section-3.3
// tslint:disable-next-line
var rfc2822 = /^(?:(Mon|Tue|Wed|Thu|Fri|Sat|Sun),?\s)?(\d{1,2})\s(Jan|Feb|Mar|Apr|May|Jun|Jul|Aug|Sep|Oct|Nov|Dec)\s(\d{2,4})\s(\d\d):(\d\d)(?::(\d\d))?\s(?:(UT|GMT|[ECMP][SD]T)|([Zz])|([+-]\d{4}))$/;
// date from iso format
function configFromISO(config) {
    if (!Object(_utils_type_checks__WEBPACK_IMPORTED_MODULE_1__["isString"])(config._i)) {
        return config;
    }
    var input = config._i;
    var match = extendedIsoRegex.exec(input) || basicIsoRegex.exec(input);
    var allowTime;
    var dateFormat;
    var timeFormat;
    var tzFormat;
    if (!match) {
        config._isValid = false;
        return config;
    }
    // getParsingFlags(config).iso = true;
    var i;
    var l;
    for (i = 0, l = isoDates.length; i < l; i++) {
        if (isoDates[i][1].exec(match[1])) {
            dateFormat = isoDates[i][0];
            allowTime = isoDates[i][2] !== false;
            break;
        }
    }
    if (dateFormat == null) {
        config._isValid = false;
        return config;
    }
    if (match[3]) {
        for (i = 0, l = isoTimes.length; i < l; i++) {
            if (isoTimes[i][1].exec(match[3])) {
                // match[2] should be 'T' or space
                timeFormat = (match[2] || ' ') + isoTimes[i][0];
                break;
            }
        }
        if (timeFormat == null) {
            config._isValid = false;
            return config;
        }
    }
    if (!allowTime && timeFormat != null) {
        config._isValid = false;
        return config;
    }
    if (match[4]) {
        if (tzRegex.exec(match[4])) {
            tzFormat = 'Z';
        }
        else {
            config._isValid = false;
            return config;
        }
    }
    config._f = dateFormat + (timeFormat || '') + (tzFormat || '');
    return Object(_from_string_and_format__WEBPACK_IMPORTED_MODULE_2__["configFromStringAndFormat"])(config);
}
// tslint:disable-next-line
function extractFromRFC2822Strings(yearStr, monthStr, dayStr, hourStr, minuteStr, secondStr) {
    var result = [
        untruncateYear(yearStr),
        _locale_locale_class__WEBPACK_IMPORTED_MODULE_0__["defaultLocaleMonthsShort"].indexOf(monthStr),
        parseInt(dayStr, 10),
        parseInt(hourStr, 10),
        parseInt(minuteStr, 10)
    ];
    if (secondStr) {
        result.push(parseInt(secondStr, 10));
    }
    return result;
}
function untruncateYear(yearStr) {
    var year = parseInt(yearStr, 10);
    return year <= 49 ? year + 2000 : year;
}
function preprocessRFC2822(str) {
    // Remove comments and folding whitespace and replace multiple-spaces with a single space
    return str
        .replace(/\([^)]*\)|[\n\t]/g, ' ')
        .replace(/(\s\s+)/g, ' ').trim();
}
function checkWeekday(weekdayStr, parsedInput, config) {
    if (weekdayStr) {
        // TODO: Replace the vanilla JS Date object with an indepentent day-of-week check.
        var weekdayProvided = _locale_locale_class__WEBPACK_IMPORTED_MODULE_0__["defaultLocaleWeekdaysShort"].indexOf(weekdayStr);
        var weekdayActual = new Date(parsedInput[0], parsedInput[1], parsedInput[2]).getDay();
        if (weekdayProvided !== weekdayActual) {
            Object(_parsing_flags__WEBPACK_IMPORTED_MODULE_5__["getParsingFlags"])(config).weekdayMismatch = true;
            config._isValid = false;
            return false;
        }
    }
    return true;
}
function calculateOffset(obsOffset, militaryOffset, numOffset) {
    if (obsOffset) {
        return obsOffsets[obsOffset];
    }
    else if (militaryOffset) {
        // the only allowed military tz is Z
        return 0;
    }
    else {
        var hm = parseInt(numOffset, 10);
        var m = hm % 100;
        var h = (hm - m) / 100;
        return h * 60 + m;
    }
}
// date and time from ref 2822 format
function configFromRFC2822(config) {
    if (!Object(_utils_type_checks__WEBPACK_IMPORTED_MODULE_1__["isString"])(config._i)) {
        return config;
    }
    var match = rfc2822.exec(preprocessRFC2822(config._i));
    if (!match) {
        return Object(_valid__WEBPACK_IMPORTED_MODULE_4__["markInvalid"])(config);
    }
    var parsedArray = extractFromRFC2822Strings(match[4], match[3], match[2], match[5], match[6], match[7]);
    if (!checkWeekday(match[1], parsedArray, config)) {
        return config;
    }
    config._a = parsedArray;
    config._tzm = calculateOffset(match[8], match[9], match[10]);
    config._d = _date_from_array__WEBPACK_IMPORTED_MODULE_3__["createUTCDate"].apply(null, config._a);
    config._d.setUTCMinutes(config._d.getUTCMinutes() - config._tzm);
    Object(_parsing_flags__WEBPACK_IMPORTED_MODULE_5__["getParsingFlags"])(config).rfc2822 = true;
    return config;
}
// date from iso format or fallback
function configFromString(config) {
    if (!Object(_utils_type_checks__WEBPACK_IMPORTED_MODULE_1__["isString"])(config._i)) {
        return config;
    }
    var matched = aspNetJsonRegex.exec(config._i);
    if (matched !== null) {
        config._d = new Date(+matched[1]);
        return config;
    }
    // todo: update logic processing
    // isISO -> configFromISO
    // isRFC -> configFromRFC
    configFromISO(config);
    if (config._isValid === false) {
        delete config._isValid;
    }
    else {
        return config;
    }
    configFromRFC2822(config);
    if (config._isValid === false) {
        delete config._isValid;
    }
    else {
        return config;
    }
    // Final attempt, use Input Fallback
    // hooks.createFromInputFallback(config);
    return Object(_valid__WEBPACK_IMPORTED_MODULE_4__["createInvalid"])(config);
}
// hooks.createFromInputFallback = deprecate(
//     'value provided is not in a recognized RFC2822 or ISO format. moment construction falls back to js Date(), ' +
//     'which is not reliable across all browsers and versions. Non RFC2822/ISO date formats are ' +
//     'discouraged and will be removed in an upcoming major release. Please refer to ' +
//     'http://momentjs.com/guides/#/warnings/js-date/ for more info.',
//     function (config) {
//         config._d = new Date(config._i + (config._useUTC ? ' UTC' : ''));
//     }
// );
//# sourceMappingURL=from-string.js.map

/***/ }),

/***/ "./node_modules/ngx-bootstrap/chronos/create/local.js":
/*!************************************************************!*\
  !*** ./node_modules/ngx-bootstrap/chronos/create/local.js ***!
  \************************************************************/
/*! exports provided: parseDate */
/***/ (function(module, __webpack_exports__, __webpack_require__) {

"use strict";
__webpack_require__.r(__webpack_exports__);
/* harmony export (binding) */ __webpack_require__.d(__webpack_exports__, "parseDate", function() { return parseDate; });
/* harmony import */ var _from_anything__WEBPACK_IMPORTED_MODULE_0__ = __webpack_require__(/*! ./from-anything */ "./node_modules/ngx-bootstrap/chronos/create/from-anything.js");
/* harmony import */ var _utils_type_checks__WEBPACK_IMPORTED_MODULE_1__ = __webpack_require__(/*! ../utils/type-checks */ "./node_modules/ngx-bootstrap/chronos/utils/type-checks.js");


function parseDate(input, format, localeKey, strict, isUTC) {
    if (Object(_utils_type_checks__WEBPACK_IMPORTED_MODULE_1__["isDate"])(input)) {
        return input;
    }
    var config = Object(_from_anything__WEBPACK_IMPORTED_MODULE_0__["createLocalOrUTC"])(input, format, localeKey, strict, isUTC);
    return config._d;
}
//# sourceMappingURL=local.js.map

/***/ }),

/***/ "./node_modules/ngx-bootstrap/chronos/create/parsing-flags.js":
/*!********************************************************************!*\
  !*** ./node_modules/ngx-bootstrap/chronos/create/parsing-flags.js ***!
  \********************************************************************/
/*! exports provided: getParsingFlags */
/***/ (function(module, __webpack_exports__, __webpack_require__) {

"use strict";
__webpack_require__.r(__webpack_exports__);
/* harmony export (binding) */ __webpack_require__.d(__webpack_exports__, "getParsingFlags", function() { return getParsingFlags; });
function defaultParsingFlags() {
    // We need to deep clone this object.
    return {
        empty: false,
        unusedTokens: [],
        unusedInput: [],
        overflow: -2,
        charsLeftOver: 0,
        nullInput: false,
        invalidMonth: null,
        invalidFormat: false,
        userInvalidated: false,
        iso: false,
        parsedDateParts: [],
        meridiem: null,
        rfc2822: false,
        weekdayMismatch: false
    };
}
function getParsingFlags(config) {
    if (config._pf == null) {
        config._pf = defaultParsingFlags();
    }
    return config._pf;
}
//# sourceMappingURL=parsing-flags.js.map

/***/ }),

/***/ "./node_modules/ngx-bootstrap/chronos/create/valid.js":
/*!************************************************************!*\
  !*** ./node_modules/ngx-bootstrap/chronos/create/valid.js ***!
  \************************************************************/
/*! exports provided: isValid, createInvalid, markInvalid */
/***/ (function(module, __webpack_exports__, __webpack_require__) {

"use strict";
__webpack_require__.r(__webpack_exports__);
/* harmony export (binding) */ __webpack_require__.d(__webpack_exports__, "isValid", function() { return isValid; });
/* harmony export (binding) */ __webpack_require__.d(__webpack_exports__, "createInvalid", function() { return createInvalid; });
/* harmony export (binding) */ __webpack_require__.d(__webpack_exports__, "markInvalid", function() { return markInvalid; });
/* harmony import */ var _parsing_flags__WEBPACK_IMPORTED_MODULE_0__ = __webpack_require__(/*! ./parsing-flags */ "./node_modules/ngx-bootstrap/chronos/create/parsing-flags.js");

function isValid(config) {
    if (config._isValid == null) {
        var flags = Object(_parsing_flags__WEBPACK_IMPORTED_MODULE_0__["getParsingFlags"])(config);
        var parsedParts = Array.prototype.some.call(flags.parsedDateParts, function (i) {
            return i != null;
        });
        var isNowValid = !isNaN(config._d && config._d.getTime()) &&
            flags.overflow < 0 &&
            !flags.empty &&
            !flags.invalidMonth &&
            !flags.invalidWeekday &&
            !flags.weekdayMismatch &&
            !flags.nullInput &&
            !flags.invalidFormat &&
            !flags.userInvalidated &&
            (!flags.meridiem || (flags.meridiem && parsedParts));
        if (config._strict) {
            isNowValid = isNowValid &&
                flags.charsLeftOver === 0 &&
                flags.unusedTokens.length === 0 &&
                flags.bigHour === undefined;
        }
        if (Object.isFrozen == null || !Object.isFrozen(config)) {
            config._isValid = isNowValid;
        }
        else {
            return isNowValid;
        }
    }
    return config._isValid;
}
function createInvalid(config, flags) {
    config._d = new Date(NaN);
    Object.assign(Object(_parsing_flags__WEBPACK_IMPORTED_MODULE_0__["getParsingFlags"])(config), flags || { userInvalidated: true });
    return config;
}
function markInvalid(config) {
    config._isValid = false;
    return config;
}
//# sourceMappingURL=valid.js.map

/***/ }),

/***/ "./node_modules/ngx-bootstrap/chronos/duration/bubble.js":
/*!***************************************************************!*\
  !*** ./node_modules/ngx-bootstrap/chronos/duration/bubble.js ***!
  \***************************************************************/
/*! exports provided: bubble, daysToMonths, monthsToDays */
/***/ (function(module, __webpack_exports__, __webpack_require__) {

"use strict";
__webpack_require__.r(__webpack_exports__);
/* harmony export (binding) */ __webpack_require__.d(__webpack_exports__, "bubble", function() { return bubble; });
/* harmony export (binding) */ __webpack_require__.d(__webpack_exports__, "daysToMonths", function() { return daysToMonths; });
/* harmony export (binding) */ __webpack_require__.d(__webpack_exports__, "monthsToDays", function() { return monthsToDays; });
/* harmony import */ var _utils__WEBPACK_IMPORTED_MODULE_0__ = __webpack_require__(/*! ../utils */ "./node_modules/ngx-bootstrap/chronos/utils.js");
/* harmony import */ var _utils_abs_ceil__WEBPACK_IMPORTED_MODULE_1__ = __webpack_require__(/*! ../utils/abs-ceil */ "./node_modules/ngx-bootstrap/chronos/utils/abs-ceil.js");


function bubble(dur) {
    var milliseconds = dur._milliseconds;
    var days = dur._days;
    var months = dur._months;
    var data = dur._data;
    // if we have a mix of positive and negative values, bubble down first
    // check: https://github.com/moment/moment/issues/2166
    if (!((milliseconds >= 0 && days >= 0 && months >= 0) ||
        (milliseconds <= 0 && days <= 0 && months <= 0))) {
        milliseconds += Object(_utils_abs_ceil__WEBPACK_IMPORTED_MODULE_1__["absCeil"])(monthsToDays(months) + days) * 864e5;
        days = 0;
        months = 0;
    }
    // The following code bubbles up values, see the tests for
    // examples of what that means.
    data.milliseconds = milliseconds % 1000;
    var seconds = Object(_utils__WEBPACK_IMPORTED_MODULE_0__["absFloor"])(milliseconds / 1000);
    data.seconds = seconds % 60;
    var minutes = Object(_utils__WEBPACK_IMPORTED_MODULE_0__["absFloor"])(seconds / 60);
    data.minutes = minutes % 60;
    var hours = Object(_utils__WEBPACK_IMPORTED_MODULE_0__["absFloor"])(minutes / 60);
    data.hours = hours % 24;
    days += Object(_utils__WEBPACK_IMPORTED_MODULE_0__["absFloor"])(hours / 24);
    // convert days to months
    var monthsFromDays = Object(_utils__WEBPACK_IMPORTED_MODULE_0__["absFloor"])(daysToMonths(days));
    months += monthsFromDays;
    days -= Object(_utils_abs_ceil__WEBPACK_IMPORTED_MODULE_1__["absCeil"])(monthsToDays(monthsFromDays));
    // 12 months -> 1 year
    var years = Object(_utils__WEBPACK_IMPORTED_MODULE_0__["absFloor"])(months / 12);
    months %= 12;
    data.day = days;
    data.month = months;
    data.year = years;
    return dur;
}
function daysToMonths(day) {
    // 400 years have 146097 days (taking into account leap year rules)
    // 400 years have 12 months === 4800
    return day * 4800 / 146097;
}
function monthsToDays(month) {
    // the reverse of daysToMonths
    return month * 146097 / 4800;
}
//# sourceMappingURL=bubble.js.map

/***/ }),

/***/ "./node_modules/ngx-bootstrap/chronos/duration/constructor.js":
/*!********************************************************************!*\
  !*** ./node_modules/ngx-bootstrap/chronos/duration/constructor.js ***!
  \********************************************************************/
/*! exports provided: Duration, isDuration */
/***/ (function(module, __webpack_exports__, __webpack_require__) {

"use strict";
__webpack_require__.r(__webpack_exports__);
/* harmony export (binding) */ __webpack_require__.d(__webpack_exports__, "Duration", function() { return Duration; });
/* harmony export (binding) */ __webpack_require__.d(__webpack_exports__, "isDuration", function() { return isDuration; });
/* harmony import */ var _locale_locales__WEBPACK_IMPORTED_MODULE_0__ = __webpack_require__(/*! ../locale/locales */ "./node_modules/ngx-bootstrap/chronos/locale/locales.js");
/* harmony import */ var _valid__WEBPACK_IMPORTED_MODULE_1__ = __webpack_require__(/*! ./valid */ "./node_modules/ngx-bootstrap/chronos/duration/valid.js");
/* harmony import */ var _bubble__WEBPACK_IMPORTED_MODULE_2__ = __webpack_require__(/*! ./bubble */ "./node_modules/ngx-bootstrap/chronos/duration/bubble.js");
/* harmony import */ var _units_aliases__WEBPACK_IMPORTED_MODULE_3__ = __webpack_require__(/*! ../units/aliases */ "./node_modules/ngx-bootstrap/chronos/units/aliases.js");
/* harmony import */ var _humanize__WEBPACK_IMPORTED_MODULE_4__ = __webpack_require__(/*! ./humanize */ "./node_modules/ngx-bootstrap/chronos/duration/humanize.js");
/* harmony import */ var _utils_type_checks__WEBPACK_IMPORTED_MODULE_5__ = __webpack_require__(/*! ../utils/type-checks */ "./node_modules/ngx-bootstrap/chronos/utils/type-checks.js");






var Duration = /** @class */ (function () {
    function Duration(duration, config) {
        if (config === void 0) { config = {}; }
        this._data = {};
        this._locale = Object(_locale_locales__WEBPACK_IMPORTED_MODULE_0__["getLocale"])();
        this._locale = config && config._locale || Object(_locale_locales__WEBPACK_IMPORTED_MODULE_0__["getLocale"])();
        // const normalizedInput = normalizeObjectUnits(duration);
        var normalizedInput = duration;
        var years = normalizedInput.year || 0;
        var quarters = normalizedInput.quarter || 0;
        var months = normalizedInput.month || 0;
        var weeks = normalizedInput.week || 0;
        var days = normalizedInput.day || 0;
        var hours = normalizedInput.hours || 0;
        var minutes = normalizedInput.minutes || 0;
        var seconds = normalizedInput.seconds || 0;
        var milliseconds = normalizedInput.milliseconds || 0;
        this._isValid = Object(_valid__WEBPACK_IMPORTED_MODULE_1__["isDurationValid"])(normalizedInput);
        // representation for dateAddRemove
        this._milliseconds = +milliseconds +
            seconds * 1000 +
            minutes * 60 * 1000 + // 1000 * 60
            // 1000 * 60
            hours * 1000 * 60 * 60; // using 1000 * 60 * 60
        // instead of 36e5 to avoid floating point rounding errors https://github.com/moment/moment/issues/2978
        // Because of dateAddRemove treats 24 hours as different from a
        // day when working around DST, we need to store them separately
        this._days = +days +
            weeks * 7;
        // It is impossible to translate months into days without knowing
        // which months you are are talking about, so we have to store
        // it separately.
        this._months = +months +
            quarters * 3 +
            years * 12;
        // this._data = {};
        // this._locale = getLocale();
        // this._bubble();
        return Object(_bubble__WEBPACK_IMPORTED_MODULE_2__["bubble"])(this);
    }
    Duration.prototype.isValid = function () {
        return this._isValid;
    };
    Duration.prototype.humanize = function (withSuffix) {
        // throw new Error(`TODO: implement`);
        if (!this.isValid()) {
            return this.localeData().invalidDate;
        }
        var locale = this.localeData();
        var output = Object(_humanize__WEBPACK_IMPORTED_MODULE_4__["relativeTime"])(this, !withSuffix, locale);
        if (withSuffix) {
            output = locale.pastFuture(+this, output);
        }
        return locale.postformat(output);
    };
    Duration.prototype.localeData = function () {
        return this._locale;
    };
    Duration.prototype.locale = function (localeKey) {
        if (!localeKey) {
            return this._locale._abbr;
        }
        this._locale = Object(_locale_locales__WEBPACK_IMPORTED_MODULE_0__["getLocale"])(localeKey) || this._locale;
        return this;
    };
    Duration.prototype.abs = function () {
        var mathAbs = Math.abs;
        var data = this._data;
        this._milliseconds = mathAbs(this._milliseconds);
        this._days = mathAbs(this._days);
        this._months = mathAbs(this._months);
        data.milliseconds = mathAbs(data.milliseconds);
        data.seconds = mathAbs(data.seconds);
        data.minutes = mathAbs(data.minutes);
        data.hours = mathAbs(data.hours);
        data.month = mathAbs(data.month);
        data.year = mathAbs(data.year);
        return this;
    };
    Duration.prototype.as = function (_units) {
        if (!this.isValid()) {
            return NaN;
        }
        var days;
        var months;
        var milliseconds = this._milliseconds;
        var units = Object(_units_aliases__WEBPACK_IMPORTED_MODULE_3__["normalizeUnits"])(_units);
        if (units === 'month' || units === 'year') {
            days = this._days + milliseconds / 864e5;
            months = this._months + Object(_bubble__WEBPACK_IMPORTED_MODULE_2__["daysToMonths"])(days);
            return units === 'month' ? months : months / 12;
        }
        // handle milliseconds separately because of floating point math errors (issue #1867)
        days = this._days + Math.round(Object(_bubble__WEBPACK_IMPORTED_MODULE_2__["monthsToDays"])(this._months));
        switch (units) {
            case 'week':
                return days / 7 + milliseconds / 6048e5;
            case 'day':
                return days + milliseconds / 864e5;
            case 'hours':
                return days * 24 + milliseconds / 36e5;
            case 'minutes':
                return days * 1440 + milliseconds / 6e4;
            case 'seconds':
                return days * 86400 + milliseconds / 1000;
            // Math.floor prevents floating point math errors here
            case 'milliseconds':
                return Math.floor(days * 864e5) + milliseconds;
            default:
                throw new Error("Unknown unit " + units);
        }
    };
    Duration.prototype.valueOf = function () {
        if (!this.isValid()) {
            return NaN;
        }
        return (this._milliseconds +
            this._days * 864e5 +
            (this._months % 12) * 2592e6 +
            Object(_utils_type_checks__WEBPACK_IMPORTED_MODULE_5__["toInt"])(this._months / 12) * 31536e6);
    };
    return Duration;
}());

function isDuration(obj) {
    return obj instanceof Duration;
}
//# sourceMappingURL=constructor.js.map

/***/ }),

/***/ "./node_modules/ngx-bootstrap/chronos/duration/create.js":
/*!***************************************************************!*\
  !*** ./node_modules/ngx-bootstrap/chronos/duration/create.js ***!
  \***************************************************************/
/*! exports provided: createDuration */
/***/ (function(module, __webpack_exports__, __webpack_require__) {

"use strict";
__webpack_require__.r(__webpack_exports__);
/* harmony export (binding) */ __webpack_require__.d(__webpack_exports__, "createDuration", function() { return createDuration; });
/* harmony import */ var _constructor__WEBPACK_IMPORTED_MODULE_0__ = __webpack_require__(/*! ./constructor */ "./node_modules/ngx-bootstrap/chronos/duration/constructor.js");
/* harmony import */ var _utils_type_checks__WEBPACK_IMPORTED_MODULE_1__ = __webpack_require__(/*! ../utils/type-checks */ "./node_modules/ngx-bootstrap/chronos/utils/type-checks.js");
/* harmony import */ var _units_constants__WEBPACK_IMPORTED_MODULE_2__ = __webpack_require__(/*! ../units/constants */ "./node_modules/ngx-bootstrap/chronos/units/constants.js");
/* harmony import */ var _create_local__WEBPACK_IMPORTED_MODULE_3__ = __webpack_require__(/*! ../create/local */ "./node_modules/ngx-bootstrap/chronos/create/local.js");
/* harmony import */ var _utils_abs_round__WEBPACK_IMPORTED_MODULE_4__ = __webpack_require__(/*! ../utils/abs-round */ "./node_modules/ngx-bootstrap/chronos/utils/abs-round.js");
/* harmony import */ var _units_offset__WEBPACK_IMPORTED_MODULE_5__ = __webpack_require__(/*! ../units/offset */ "./node_modules/ngx-bootstrap/chronos/units/offset.js");
/* harmony import */ var _utils_date_compare__WEBPACK_IMPORTED_MODULE_6__ = __webpack_require__(/*! ../utils/date-compare */ "./node_modules/ngx-bootstrap/chronos/utils/date-compare.js");
/* harmony import */ var _utils_date_getters__WEBPACK_IMPORTED_MODULE_7__ = __webpack_require__(/*! ../utils/date-getters */ "./node_modules/ngx-bootstrap/chronos/utils/date-getters.js");
/* harmony import */ var _moment_add_subtract__WEBPACK_IMPORTED_MODULE_8__ = __webpack_require__(/*! ../moment/add-subtract */ "./node_modules/ngx-bootstrap/chronos/moment/add-subtract.js");
/* harmony import */ var _create_clone__WEBPACK_IMPORTED_MODULE_9__ = __webpack_require__(/*! ../create/clone */ "./node_modules/ngx-bootstrap/chronos/create/clone.js");










var aspNetRegex = /^(\-|\+)?(?:(\d*)[. ])?(\d+)\:(\d+)(?:\:(\d+)(\.\d*)?)?$/;
// from http://docs.closure-library.googlecode.com/git/closure_goog_date_date.js.source.html
// somewhat more in line with 4.4.3.2 2004 spec, but allows decimal anywhere
// and further modified to allow for strings containing both week and day
// tslint:disable-next-line
var isoRegex = /^(-|\+)?P(?:([-+]?[0-9,.]*)Y)?(?:([-+]?[0-9,.]*)M)?(?:([-+]?[0-9,.]*)W)?(?:([-+]?[0-9,.]*)D)?(?:T(?:([-+]?[0-9,.]*)H)?(?:([-+]?[0-9,.]*)M)?(?:([-+]?[0-9,.]*)S)?)?$/;
function createDuration(input, key, config) {
    if (config === void 0) { config = {}; }
    var duration = convertDuration(input, key);
    // matching against regexp is expensive, do it on demand
    return new _constructor__WEBPACK_IMPORTED_MODULE_0__["Duration"](duration, config);
}
function convertDuration(input, key) {
    // checks for null or undefined
    if (input == null) {
        return {};
    }
    if (Object(_constructor__WEBPACK_IMPORTED_MODULE_0__["isDuration"])(input)) {
        return {
            milliseconds: input._milliseconds,
            day: input._days,
            month: input._months
        };
    }
    if (Object(_utils_type_checks__WEBPACK_IMPORTED_MODULE_1__["isNumber"])(input)) {
        // duration = {};
        return key ? (_a = {}, _a[key] = input, _a) : { milliseconds: input };
    }
    if (Object(_utils_type_checks__WEBPACK_IMPORTED_MODULE_1__["isString"])(input)) {
        var match = aspNetRegex.exec(input);
        if (match) {
            var sign = (match[1] === '-') ? -1 : 1;
            return {
                year: 0,
                day: Object(_utils_type_checks__WEBPACK_IMPORTED_MODULE_1__["toInt"])(match[_units_constants__WEBPACK_IMPORTED_MODULE_2__["DATE"]]) * sign,
                hours: Object(_utils_type_checks__WEBPACK_IMPORTED_MODULE_1__["toInt"])(match[_units_constants__WEBPACK_IMPORTED_MODULE_2__["HOUR"]]) * sign,
                minutes: Object(_utils_type_checks__WEBPACK_IMPORTED_MODULE_1__["toInt"])(match[_units_constants__WEBPACK_IMPORTED_MODULE_2__["MINUTE"]]) * sign,
                seconds: Object(_utils_type_checks__WEBPACK_IMPORTED_MODULE_1__["toInt"])(match[_units_constants__WEBPACK_IMPORTED_MODULE_2__["SECOND"]]) * sign,
                // the millisecond decimal point is included in the match
                milliseconds: Object(_utils_type_checks__WEBPACK_IMPORTED_MODULE_1__["toInt"])(Object(_utils_abs_round__WEBPACK_IMPORTED_MODULE_4__["absRound"])(Object(_utils_type_checks__WEBPACK_IMPORTED_MODULE_1__["toInt"])(match[_units_constants__WEBPACK_IMPORTED_MODULE_2__["MILLISECOND"]]) * 1000)) * sign
            };
        }
        match = isoRegex.exec(input);
        if (match) {
            var sign = (match[1] === '-') ? -1 : (match[1] === '+') ? 1 : 1;
            return {
                year: parseIso(match[2], sign),
                month: parseIso(match[3], sign),
                week: parseIso(match[4], sign),
                day: parseIso(match[5], sign),
                hours: parseIso(match[6], sign),
                minutes: parseIso(match[7], sign),
                seconds: parseIso(match[8], sign)
            };
        }
    }
    if (Object(_utils_type_checks__WEBPACK_IMPORTED_MODULE_1__["isObject"])(input) && ('from' in input || 'to' in input)) {
        var diffRes = momentsDifference(Object(_create_local__WEBPACK_IMPORTED_MODULE_3__["parseDate"])(input.from), Object(_create_local__WEBPACK_IMPORTED_MODULE_3__["parseDate"])(input.to));
        return {
            milliseconds: diffRes.milliseconds,
            month: diffRes.months
        };
    }
    return input;
    var _a;
}
// createDuration.fn = Duration.prototype;
// createDuration.invalid = invalid;
function parseIso(inp, sign) {
    // We'd normally use ~~inp for this, but unfortunately it also
    // converts floats to ints.
    // inp may be undefined, so careful calling replace on it.
    var res = inp && parseFloat(inp.replace(',', '.'));
    // apply sign while we're at it
    return (isNaN(res) ? 0 : res) * sign;
}
function positiveMomentsDifference(base, other) {
    var res = { milliseconds: 0, months: 0 };
    res.months = Object(_utils_date_getters__WEBPACK_IMPORTED_MODULE_7__["getMonth"])(other) - Object(_utils_date_getters__WEBPACK_IMPORTED_MODULE_7__["getMonth"])(base) +
        (Object(_utils_date_getters__WEBPACK_IMPORTED_MODULE_7__["getFullYear"])(other) - Object(_utils_date_getters__WEBPACK_IMPORTED_MODULE_7__["getFullYear"])(base)) * 12;
    var _basePlus = Object(_moment_add_subtract__WEBPACK_IMPORTED_MODULE_8__["add"])(Object(_create_clone__WEBPACK_IMPORTED_MODULE_9__["cloneDate"])(base), res.months, 'month');
    if (Object(_utils_date_compare__WEBPACK_IMPORTED_MODULE_6__["isAfter"])(_basePlus, other)) {
        --res.months;
    }
    res.milliseconds = +other - +(Object(_moment_add_subtract__WEBPACK_IMPORTED_MODULE_8__["add"])(Object(_create_clone__WEBPACK_IMPORTED_MODULE_9__["cloneDate"])(base), res.months, 'month'));
    return res;
}
function momentsDifference(base, other) {
    if (!(Object(_utils_type_checks__WEBPACK_IMPORTED_MODULE_1__["isDateValid"])(base) && Object(_utils_type_checks__WEBPACK_IMPORTED_MODULE_1__["isDateValid"])(other))) {
        return { milliseconds: 0, months: 0 };
    }
    var res;
    var _other = Object(_units_offset__WEBPACK_IMPORTED_MODULE_5__["cloneWithOffset"])(other, base, { _offset: base.getTimezoneOffset() });
    if (Object(_utils_date_compare__WEBPACK_IMPORTED_MODULE_6__["isBefore"])(base, _other)) {
        res = positiveMomentsDifference(base, _other);
    }
    else {
        res = positiveMomentsDifference(_other, base);
        res.milliseconds = -res.milliseconds;
        res.months = -res.months;
    }
    return res;
}
//# sourceMappingURL=create.js.map

/***/ }),

/***/ "./node_modules/ngx-bootstrap/chronos/duration/humanize.js":
/*!*****************************************************************!*\
  !*** ./node_modules/ngx-bootstrap/chronos/duration/humanize.js ***!
  \*****************************************************************/
/*! exports provided: relativeTime, getSetRelativeTimeRounding, getSetRelativeTimeThreshold */
/***/ (function(module, __webpack_exports__, __webpack_require__) {

"use strict";
__webpack_require__.r(__webpack_exports__);
/* harmony export (binding) */ __webpack_require__.d(__webpack_exports__, "relativeTime", function() { return relativeTime; });
/* harmony export (binding) */ __webpack_require__.d(__webpack_exports__, "getSetRelativeTimeRounding", function() { return getSetRelativeTimeRounding; });
/* harmony export (binding) */ __webpack_require__.d(__webpack_exports__, "getSetRelativeTimeThreshold", function() { return getSetRelativeTimeThreshold; });
/* harmony import */ var _create__WEBPACK_IMPORTED_MODULE_0__ = __webpack_require__(/*! ./create */ "./node_modules/ngx-bootstrap/chronos/duration/create.js");

var round = Math.round;
var thresholds = {
    ss: 44,
    // a few seconds to seconds
    s: 45,
    // seconds to minute
    m: 45,
    // minutes to hour
    h: 22,
    // hours to day
    d: 26,
    // days to month
    M: 11 // months to year
};
// helper function for moment.fn.from, moment.fn.fromNow, and moment.duration.fn.humanize
function substituteTimeAgo(str, num, withoutSuffix, isFuture, locale) {
    return locale.relativeTime(num || 1, !!withoutSuffix, str, isFuture);
}
function relativeTime(posNegDuration, withoutSuffix, locale) {
    var duration = Object(_create__WEBPACK_IMPORTED_MODULE_0__["createDuration"])(posNegDuration).abs();
    var seconds = round(duration.as('s'));
    var minutes = round(duration.as('m'));
    var hours = round(duration.as('h'));
    var days = round(duration.as('d'));
    var months = round(duration.as('M'));
    var years = round(duration.as('y'));
    var a = seconds <= thresholds.ss && ['s', seconds] ||
        seconds < thresholds.s && ['ss', seconds] ||
        minutes <= 1 && ['m'] ||
        minutes < thresholds.m && ['mm', minutes] ||
        hours <= 1 && ['h'] ||
        hours < thresholds.h && ['hh', hours] ||
        days <= 1 && ['d'] ||
        days < thresholds.d && ['dd', days] ||
        months <= 1 && ['M'] ||
        months < thresholds.M && ['MM', months] ||
        years <= 1 && ['y'] || ['yy', years];
    var b = [a[0], a[1], withoutSuffix, +posNegDuration > 0, locale];
    // a[2] = withoutSuffix;
    // a[3] = +posNegDuration > 0;
    // a[4] = locale;
    return substituteTimeAgo.apply(null, b);
}
// This function allows you to set the rounding function for relative time strings
function getSetRelativeTimeRounding(roundingFunction) {
    if (roundingFunction === undefined) {
        return round;
    }
    if (typeof (roundingFunction) === 'function') {
        round = roundingFunction;
        return true;
    }
    return false;
}
// This function allows you to set a threshold for relative time strings
function getSetRelativeTimeThreshold(threshold, limit) {
    if (thresholds[threshold] === undefined) {
        return false;
    }
    if (limit === undefined) {
        return thresholds[threshold];
    }
    thresholds[threshold] = limit;
    if (threshold === 's') {
        thresholds.ss = limit - 1;
    }
    return true;
}
// export function humanize(withSuffix) {
//   if (!this.isValid()) {
//     return this.localeData().invalidDate();
//   }
//
//   const locale = this.localeData();
//   let output = relativeTime(this, !withSuffix, locale);
//
//   if (withSuffix) {
//     output = locale.pastFuture(+this, output);
//   }
//
//   return locale.postformat(output);
// }
//# sourceMappingURL=humanize.js.map

/***/ }),

/***/ "./node_modules/ngx-bootstrap/chronos/duration/valid.js":
/*!**************************************************************!*\
  !*** ./node_modules/ngx-bootstrap/chronos/duration/valid.js ***!
  \**************************************************************/
/*! exports provided: isDurationValid, ɵ0 */
/***/ (function(module, __webpack_exports__, __webpack_require__) {

"use strict";
__webpack_require__.r(__webpack_exports__);
/* harmony export (binding) */ __webpack_require__.d(__webpack_exports__, "isDurationValid", function() { return isDurationValid; });
/* harmony export (binding) */ __webpack_require__.d(__webpack_exports__, "ɵ0", function() { return ɵ0; });
/* harmony import */ var _utils_type_checks__WEBPACK_IMPORTED_MODULE_0__ = __webpack_require__(/*! ../utils/type-checks */ "./node_modules/ngx-bootstrap/chronos/utils/type-checks.js");

var ordering = ['year', 'quarter', 'month', 'week', 'day', 'hours', 'minutes', 'seconds', 'milliseconds'];
var ɵ0 = function (mem, order) {
    mem[order] = true;
    return mem;
};
var orderingHash = ordering.reduce(ɵ0, {});
function isDurationValid(duration) {
    var durationKeys = Object.keys(duration);
    if (durationKeys
        .some(function (key) {
        return (key in orderingHash)
            && duration[key] === null
            || isNaN(duration[key]);
    })) {
        return false;
    }
    // for (let key in duration) {
    //   if (!(indexOf.call(ordering, key) !== -1 && (duration[key] == null || !isNaN(duration[key])))) {
    //     return false;
    //   }
    // }
    var unitHasDecimal = false;
    for (var i = 0; i < ordering.length; ++i) {
        if (duration[ordering[i]]) {
            // only allow non-integers for smallest unit
            if (unitHasDecimal) {
                return false;
            }
            if (duration[ordering[i]] !== Object(_utils_type_checks__WEBPACK_IMPORTED_MODULE_0__["toInt"])(duration[ordering[i]])) {
                unitHasDecimal = true;
            }
        }
    }
    return true;
}
// export function isValid() {
//   return this._isValid;
// }
//
// export function createInvalid(): Duration {
//   return createDuration(NaN);
// }

//# sourceMappingURL=valid.js.map

/***/ }),

/***/ "./node_modules/ngx-bootstrap/chronos/format.js":
/*!******************************************************!*\
  !*** ./node_modules/ngx-bootstrap/chronos/format.js ***!
  \******************************************************/
/*! exports provided: formatDate, formatMoment, expandFormat */
/***/ (function(module, __webpack_exports__, __webpack_require__) {

"use strict";
__webpack_require__.r(__webpack_exports__);
/* harmony export (binding) */ __webpack_require__.d(__webpack_exports__, "formatDate", function() { return formatDate; });
/* harmony export (binding) */ __webpack_require__.d(__webpack_exports__, "formatMoment", function() { return formatMoment; });
/* harmony export (binding) */ __webpack_require__.d(__webpack_exports__, "expandFormat", function() { return expandFormat; });
/* harmony import */ var _units_index__WEBPACK_IMPORTED_MODULE_0__ = __webpack_require__(/*! ./units/index */ "./node_modules/ngx-bootstrap/chronos/units/index.js");
/* harmony import */ var _format_format__WEBPACK_IMPORTED_MODULE_1__ = __webpack_require__(/*! ./format/format */ "./node_modules/ngx-bootstrap/chronos/format/format.js");
/* harmony import */ var _locale_locales__WEBPACK_IMPORTED_MODULE_2__ = __webpack_require__(/*! ./locale/locales */ "./node_modules/ngx-bootstrap/chronos/locale/locales.js");
/* harmony import */ var _utils_type_checks__WEBPACK_IMPORTED_MODULE_3__ = __webpack_require__(/*! ./utils/type-checks */ "./node_modules/ngx-bootstrap/chronos/utils/type-checks.js");
// moment.js
// version : 2.18.1
// authors : Tim Wood, Iskren Chernev, Moment.js contributors
// license : MIT
// momentjs.com




function formatDate(date, format, locale, isUTC, offset) {
    if (offset === void 0) { offset = 0; }
    var _locale = Object(_locale_locales__WEBPACK_IMPORTED_MODULE_2__["getLocale"])(locale || 'en');
    if (!_locale) {
        throw new Error("Locale \"" + locale + "\" is not defined, please add it with \"defineLocale(...)\"");
    }
    var _format = format || (isUTC ? 'YYYY-MM-DDTHH:mm:ss[Z]' : 'YYYY-MM-DDTHH:mm:ssZ');
    var output = formatMoment(date, _format, _locale, isUTC, offset);
    if (!output) {
        return output;
    }
    return _locale.postformat(output);
}
// format date using native date object
function formatMoment(date, _format, locale, isUTC, offset) {
    if (offset === void 0) { offset = 0; }
    if (!Object(_utils_type_checks__WEBPACK_IMPORTED_MODULE_3__["isDateValid"])(date)) {
        return locale.invalidDate;
    }
    var format = expandFormat(_format, locale);
    _format_format__WEBPACK_IMPORTED_MODULE_1__["formatFunctions"][format] = _format_format__WEBPACK_IMPORTED_MODULE_1__["formatFunctions"][format] || Object(_format_format__WEBPACK_IMPORTED_MODULE_1__["makeFormatFunction"])(format);
    return _format_format__WEBPACK_IMPORTED_MODULE_1__["formatFunctions"][format](date, locale, isUTC, offset);
}
function expandFormat(_format, locale) {
    var format = _format;
    var i = 5;
    var localFormattingTokens = /(\[[^\[]*\])|(\\)?(LTS|LT|LL?L?L?|l{1,4})/g;
    var replaceLongDateFormatTokens = function (input) {
        return locale.formatLongDate(input) || input;
    };
    localFormattingTokens.lastIndex = 0;
    while (i >= 0 && localFormattingTokens.test(format)) {
        format = format.replace(localFormattingTokens, replaceLongDateFormatTokens);
        localFormattingTokens.lastIndex = 0;
        i -= 1;
    }
    return format;
}
//# sourceMappingURL=format.js.map

/***/ }),

/***/ "./node_modules/ngx-bootstrap/chronos/format/format.js":
/*!*************************************************************!*\
  !*** ./node_modules/ngx-bootstrap/chronos/format/format.js ***!
  \*************************************************************/
/*! exports provided: formatFunctions, formatTokenFunctions, formattingTokens, addFormatToken, makeFormatFunction */
/***/ (function(module, __webpack_exports__, __webpack_require__) {

"use strict";
__webpack_require__.r(__webpack_exports__);
/* harmony export (binding) */ __webpack_require__.d(__webpack_exports__, "formatFunctions", function() { return formatFunctions; });
/* harmony export (binding) */ __webpack_require__.d(__webpack_exports__, "formatTokenFunctions", function() { return formatTokenFunctions; });
/* harmony export (binding) */ __webpack_require__.d(__webpack_exports__, "formattingTokens", function() { return formattingTokens; });
/* harmony export (binding) */ __webpack_require__.d(__webpack_exports__, "addFormatToken", function() { return addFormatToken; });
/* harmony export (binding) */ __webpack_require__.d(__webpack_exports__, "makeFormatFunction", function() { return makeFormatFunction; });
/* harmony import */ var _utils_zero_fill__WEBPACK_IMPORTED_MODULE_0__ = __webpack_require__(/*! ../utils/zero-fill */ "./node_modules/ngx-bootstrap/chronos/utils/zero-fill.js");
/* harmony import */ var _utils_type_checks__WEBPACK_IMPORTED_MODULE_1__ = __webpack_require__(/*! ../utils/type-checks */ "./node_modules/ngx-bootstrap/chronos/utils/type-checks.js");


var formatFunctions = {};
var formatTokenFunctions = {};
// tslint:disable-next-line
var formattingTokens = /(\[[^\[]*\])|(\\)?([Hh]mm(ss)?|Mo|MM?M?M?|Do|DDDo|DD?D?D?|ddd?d?|do?|w[o|w]?|W[o|W]?|Qo?|YYYYYY|YYYYY|YYYY|YY|gg(ggg?)?|GG(GGG?)?|e|E|a|A|hh?|HH?|kk?|mm?|ss?|S{1,9}|x|X|zz?|ZZ?|.)/g;
// token:    'M'
// padded:   ['MM', 2]
// ordinal:  'Mo'
// callback: function () { this.month() + 1 }
function addFormatToken(token, padded, ordinal, callback) {
    if (token) {
        formatTokenFunctions[token] = callback;
    }
    if (padded) {
        formatTokenFunctions[padded[0]] = function () {
            return Object(_utils_zero_fill__WEBPACK_IMPORTED_MODULE_0__["zeroFill"])(callback.apply(null, arguments), padded[1], padded[2]);
        };
    }
    if (ordinal) {
        formatTokenFunctions[ordinal] = function (date, opts) {
            return opts.locale.ordinal(callback.apply(null, arguments), token);
        };
    }
}
function makeFormatFunction(format) {
    var array = format.match(formattingTokens);
    var length = array.length;
    var formatArr = new Array(length);
    for (var i = 0; i < length; i++) {
        formatArr[i] = formatTokenFunctions[array[i]]
            ? formatTokenFunctions[array[i]]
            : removeFormattingTokens(array[i]);
    }
    return function (date, locale, isUTC, offset) {
        if (offset === void 0) { offset = 0; }
        var output = '';
        for (var j = 0; j < length; j++) {
            output += Object(_utils_type_checks__WEBPACK_IMPORTED_MODULE_1__["isFunction"])(formatArr[j])
                ? formatArr[j].call(null, date, { format: format, locale: locale, isUTC: isUTC, offset: offset })
                : formatArr[j];
        }
        return output;
    };
}
function removeFormattingTokens(input) {
    if (input.match(/\[[\s\S]/)) {
        return input.replace(/^\[|\]$/g, '');
    }
    return input.replace(/\\/g, '');
}
//# sourceMappingURL=format.js.map

/***/ }),

/***/ "./node_modules/ngx-bootstrap/chronos/i18n/ar.js":
/*!*******************************************************!*\
  !*** ./node_modules/ngx-bootstrap/chronos/i18n/ar.js ***!
  \*******************************************************/
/*! exports provided: arLocale, ɵ0, ɵ1 */
/***/ (function(module, __webpack_exports__, __webpack_require__) {

"use strict";
__webpack_require__.r(__webpack_exports__);
/* harmony export (binding) */ __webpack_require__.d(__webpack_exports__, "arLocale", function() { return arLocale; });
/* harmony export (binding) */ __webpack_require__.d(__webpack_exports__, "ɵ0", function() { return ɵ0; });
/* harmony export (binding) */ __webpack_require__.d(__webpack_exports__, "ɵ1", function() { return ɵ1; });
// tslint:disable:comment-format binary-expression-operand-order max-line-length
var symbolMap = {
    1: '١',
    2: '٢',
    3: '٣',
    4: '٤',
    5: '٥',
    6: '٦',
    7: '٧',
    8: '٨',
    9: '٩',
    0: '٠'
};
var numberMap = {
    '١': '1',
    '٢': '2',
    '٣': '3',
    '٤': '4',
    '٥': '5',
    '٦': '6',
    '٧': '7',
    '٨': '8',
    '٩': '9',
    '٠': '0'
};
var pluralForm = function (num) {
    return num === 0 ? 0 : num === 1 ? 1 : num === 2 ? 2 : num % 100 >= 3 && num % 100 <= 10 ? 3 : num % 100 >= 11 ? 4 : 5;
};
var ɵ0 = pluralForm;
var plurals = {
    s: ['أقل من ثانية', 'ثانية واحدة', ['ثانيتان', 'ثانيتين'], '%d ثوان', '%d ثانية', '%d ثانية'],
    m: ['أقل من دقيقة', 'دقيقة واحدة', ['دقيقتان', 'دقيقتين'], '%d دقائق', '%d دقيقة', '%d دقيقة'],
    h: ['أقل من ساعة', 'ساعة واحدة', ['ساعتان', 'ساعتين'], '%d ساعات', '%d ساعة', '%d ساعة'],
    d: ['أقل من يوم', 'يوم واحد', ['يومان', 'يومين'], '%d أيام', '%d يومًا', '%d يوم'],
    M: ['أقل من شهر', 'شهر واحد', ['شهران', 'شهرين'], '%d أشهر', '%d شهرا', '%d شهر'],
    y: ['أقل من عام', 'عام واحد', ['عامان', 'عامين'], '%d أعوام', '%d عامًا', '%d عام']
};
var pluralize = function (u) {
    return function (num, withoutSuffix) {
        var f = pluralForm(num);
        var str = plurals[u][pluralForm(num)];
        if (f === 2) {
            str = str[withoutSuffix ? 0 : 1];
        }
        return str.replace(/%d/i, num.toString());
    };
};
var ɵ1 = pluralize;
var months = [
    'يناير',
    'فبراير',
    'مارس',
    'أبريل',
    'مايو',
    'يونيو',
    'يوليو',
    'أغسطس',
    'سبتمبر',
    'أكتوبر',
    'نوفمبر',
    'ديسمبر'
];
var arLocale = {
    abbr: 'ar',
    months: months,
    monthsShort: months,
    weekdays: 'الأحد_الإثنين_الثلاثاء_الأربعاء_الخميس_الجمعة_السبت'.split('_'),
    weekdaysShort: 'أحد_إثنين_ثلاثاء_أربعاء_خميس_جمعة_سبت'.split('_'),
    weekdaysMin: 'ح_ن_ث_ر_خ_ج_س'.split('_'),
    weekdaysParseExact: true,
    longDateFormat: {
        LT: 'HH:mm',
        LTS: 'HH:mm:ss',
        L: 'D/\u200FM/\u200FYYYY',
        LL: 'D MMMM YYYY',
        LLL: 'D MMMM YYYY HH:mm',
        LLLL: 'dddd D MMMM YYYY HH:mm'
    },
    meridiemParse: /ص|م/,
    isPM: function (input) {
        return 'م' === input;
    },
    meridiem: function (hour, minute, isLower) {
        if (hour < 12) {
            return 'ص';
        }
        else {
            return 'م';
        }
    },
    calendar: {
        sameDay: '[اليوم عند الساعة] LT',
        nextDay: '[غدًا عند الساعة] LT',
        nextWeek: 'dddd [عند الساعة] LT',
        lastDay: '[أمس عند الساعة] LT',
        lastWeek: 'dddd [عند الساعة] LT',
        sameElse: 'L'
    },
    relativeTime: {
        future: 'بعد %s',
        past: 'منذ %s',
        s: pluralize('s'),
        ss: pluralize('s'),
        m: pluralize('m'),
        mm: pluralize('m'),
        h: pluralize('h'),
        hh: pluralize('h'),
        d: pluralize('d'),
        dd: pluralize('d'),
        M: pluralize('M'),
        MM: pluralize('M'),
        y: pluralize('y'),
        yy: pluralize('y')
    },
    preparse: function (str) {
        return str.replace(/[١٢٣٤٥٦٧٨٩٠]/g, function (match) {
            return numberMap[match];
        }).replace(/،/g, ',');
    },
    postformat: function (str) {
        return str.replace(/\d/g, function (match) {
            return symbolMap[match];
        }).replace(/,/g, '،');
    },
    week: {
        dow: 6,
        // Saturday is the first day of the week.
        doy: 12 // The week that contains Jan 1st is the first week of the year.
    }
};

//# sourceMappingURL=ar.js.map

/***/ }),

/***/ "./node_modules/ngx-bootstrap/chronos/i18n/cs.js":
/*!*******************************************************!*\
  !*** ./node_modules/ngx-bootstrap/chronos/i18n/cs.js ***!
  \*******************************************************/
/*! exports provided: csLocale */
/***/ (function(module, __webpack_exports__, __webpack_require__) {

"use strict";
__webpack_require__.r(__webpack_exports__);
/* harmony export (binding) */ __webpack_require__.d(__webpack_exports__, "csLocale", function() { return csLocale; });
/* harmony import */ var _units_day_of_week__WEBPACK_IMPORTED_MODULE_0__ = __webpack_require__(/*! ../units/day-of-week */ "./node_modules/ngx-bootstrap/chronos/units/day-of-week.js");
// tslint:disable:comment-format binary-expression-operand-order max-line-length
// tslint:disable:no-bitwise prefer-template cyclomatic-complexity
// tslint:disable:no-shadowed-variable switch-default prefer-const
// tslint:disable:one-variable-per-declaration newline-before-return

//! moment.js locale configuration
//! locale : Czech [cs]
//! author : petrbela : https://github.com/petrbela
var months = 'leden_únor_březen_duben_květen_červen_červenec_srpen_září_říjen_listopad_prosinec'.split('_');
var monthsShort = 'led_úno_bře_dub_kvě_čvn_čvc_srp_zář_říj_lis_pro'.split('_');
function plural(num) {
    return (num > 1) && (num < 5) && (~~(num / 10) !== 1);
}
function translate(num, withoutSuffix, key, isFuture) {
    var result = num + ' ';
    switch (key) {
        case 's':
            // a few seconds / in a few seconds / a few seconds ago
            return (withoutSuffix || isFuture) ? 'pár sekund' : 'pár sekundami';
        case 'ss':
            // 9 seconds / in 9 seconds / 9 seconds ago
            if (withoutSuffix || isFuture) {
                return result + (plural(num) ? 'sekundy' : 'sekund');
            }
            else {
                return result + 'sekundami';
            }
        // break;
        case 'm':
            // a minute / in a minute / a minute ago
            return withoutSuffix ? 'minuta' : (isFuture ? 'minutu' : 'minutou');
        case 'mm':
            // 9 minutes / in 9 minutes / 9 minutes ago
            if (withoutSuffix || isFuture) {
                return result + (plural(num) ? 'minuty' : 'minut');
            }
            else {
                return result + 'minutami';
            }
        // break;
        case 'h':
            // an hour / in an hour / an hour ago
            return withoutSuffix ? 'hodina' : (isFuture ? 'hodinu' : 'hodinou');
        case 'hh':
            // 9 hours / in 9 hours / 9 hours ago
            if (withoutSuffix || isFuture) {
                return result + (plural(num) ? 'hodiny' : 'hodin');
            }
            else {
                return result + 'hodinami';
            }
        // break;
        case 'd':
            // a day / in a day / a day ago
            return (withoutSuffix || isFuture) ? 'den' : 'dnem';
        case 'dd':
            // 9 days / in 9 days / 9 days ago
            if (withoutSuffix || isFuture) {
                return result + (plural(num) ? 'dny' : 'dní');
            }
            else {
                return result + 'dny';
            }
        // break;
        case 'M':
            // a month / in a month / a month ago
            return (withoutSuffix || isFuture) ? 'měsíc' : 'měsícem';
        case 'MM':
            // 9 months / in 9 months / 9 months ago
            if (withoutSuffix || isFuture) {
                return result + (plural(num) ? 'měsíce' : 'měsíců');
            }
            else {
                return result + 'měsíci';
            }
        // break;
        case 'y':
            // a year / in a year / a year ago
            return (withoutSuffix || isFuture) ? 'rok' : 'rokem';
        case 'yy':
            // 9 years / in 9 years / 9 years ago
            if (withoutSuffix || isFuture) {
                return result + (plural(num) ? 'roky' : 'let');
            }
            else {
                return result + 'lety';
            }
    }
}
var csLocale = {
    abbr: 'cs',
    months: months,
    monthsShort: monthsShort,
    monthsParse: (function (months, monthsShort) {
        var i, _monthsParse = [];
        for (i = 0; i < 12; i++) {
            // use custom parser to solve problem with July (červenec)
            _monthsParse[i] = new RegExp('^' + months[i] + '$|^' + monthsShort[i] + '$', 'i');
        }
        return _monthsParse;
    }(months, monthsShort)),
    shortMonthsParse: (function (monthsShort) {
        var i, _shortMonthsParse = [];
        for (i = 0; i < 12; i++) {
            _shortMonthsParse[i] = new RegExp('^' + monthsShort[i] + '$', 'i');
        }
        return _shortMonthsParse;
    }(monthsShort)),
    longMonthsParse: (function (months) {
        var i, _longMonthsParse = [];
        for (i = 0; i < 12; i++) {
            _longMonthsParse[i] = new RegExp('^' + months[i] + '$', 'i');
        }
        return _longMonthsParse;
    }(months)),
    weekdays: 'neděle_pondělí_úterý_středa_čtvrtek_pátek_sobota'.split('_'),
    weekdaysShort: 'ne_po_út_st_čt_pá_so'.split('_'),
    weekdaysMin: 'ne_po_út_st_čt_pá_so'.split('_'),
    longDateFormat: {
        LT: 'H:mm',
        LTS: 'H:mm:ss',
        L: 'DD.MM.YYYY',
        LL: 'D. MMMM YYYY',
        LLL: 'D. MMMM YYYY H:mm',
        LLLL: 'dddd D. MMMM YYYY H:mm',
        l: 'D. M. YYYY'
    },
    calendar: {
        sameDay: '[dnes v] LT',
        nextDay: '[zítra v] LT',
        nextWeek: function (date) {
            switch (Object(_units_day_of_week__WEBPACK_IMPORTED_MODULE_0__["getDayOfWeek"])(date)) {
                case 0:
                    return '[v neděli v] LT';
                case 1:
                case 2:
                    return '[v] dddd [v] LT';
                case 3:
                    return '[ve středu v] LT';
                case 4:
                    return '[ve čtvrtek v] LT';
                case 5:
                    return '[v pátek v] LT';
                case 6:
                    return '[v sobotu v] LT';
            }
        },
        lastDay: '[včera v] LT',
        lastWeek: function (date) {
            switch (Object(_units_day_of_week__WEBPACK_IMPORTED_MODULE_0__["getDayOfWeek"])(date)) {
                case 0:
                    return '[minulou neděli v] LT';
                case 1:
                case 2:
                    return '[minulé] dddd [v] LT';
                case 3:
                    return '[minulou středu v] LT';
                case 4:
                case 5:
                    return '[minulý] dddd [v] LT';
                case 6:
                    return '[minulou sobotu v] LT';
            }
        },
        sameElse: 'L'
    },
    relativeTime: {
        future: 'za %s',
        past: 'před %s',
        s: translate,
        ss: translate,
        m: translate,
        mm: translate,
        h: translate,
        hh: translate,
        d: translate,
        dd: translate,
        M: translate,
        MM: translate,
        y: translate,
        yy: translate
    },
    dayOfMonthOrdinalParse: /\d{1,2}\./,
    ordinal: '%d.',
    week: {
        dow: 1,
        // Monday is the first day of the week.
        doy: 4 // The week that contains Jan 4th is the first week of the year.
    }
};
//# sourceMappingURL=cs.js.map

/***/ }),

/***/ "./node_modules/ngx-bootstrap/chronos/i18n/da.js":
/*!*******************************************************!*\
  !*** ./node_modules/ngx-bootstrap/chronos/i18n/da.js ***!
  \*******************************************************/
/*! exports provided: daLocale */
/***/ (function(module, __webpack_exports__, __webpack_require__) {

"use strict";
__webpack_require__.r(__webpack_exports__);
/* harmony export (binding) */ __webpack_require__.d(__webpack_exports__, "daLocale", function() { return daLocale; });
// tslint:disable:comment-format
//! moment.js locale configuration
//! locale : Danish (Denmark) [da]
//! author : Per Hansen : https://github.com/perhp
var daLocale = {
    abbr: 'da',
    months: 'Januar_Februar_Marts_April_Maj_Juni_Juli_August_September_Oktober_November_December'.split('_'),
    monthsShort: 'Jan_Feb_Mar_Apr_Maj_Jun_Jul_Aug_Sep_Okt_Nov_Dec'.split('_'),
    weekdays: 'Søndag_Mandag_Tirsdag_Onsdag_Torsdag_Fredag_Lørdag'.split('_'),
    weekdaysShort: 'Søn_Man_Tir_Ons_Tor_Fre_Lør'.split('_'),
    weekdaysMin: 'Sø_Ma_Ti_On_To_Fr_Lø'.split('_'),
    longDateFormat: {
        LT: 'HH:mm',
        LTS: 'HH:mm:ss',
        L: 'DD/MM/YYYY',
        LL: 'D. MMMM YYYY',
        LLL: 'D. MMMM YYYY HH:mm',
        LLLL: 'dddd [d.] D. MMMM YYYY [kl.] HH:mm'
    },
    calendar: {
        sameDay: '[i dag kl.] LT',
        nextDay: '[i morgen kl.] LT',
        nextWeek: 'på dddd [kl.] LT',
        lastDay: '[i går kl.] LT',
        lastWeek: '[i] dddd[s kl.] LT',
        sameElse: 'L'
    },
    relativeTime: {
        future: 'om %s',
        past: '%s siden',
        s: 'få sekunder',
        m: 'et minut',
        mm: '%d minutter',
        h: 'en time',
        hh: '%d timer',
        d: 'en dag',
        dd: '%d dage',
        M: 'en måned',
        MM: '%d måneder',
        y: 'et år',
        yy: '%d år'
    },
    dayOfMonthOrdinalParse: /\d{1,2}\./,
    ordinal: '%d.',
    week: {
        dow: 1,
        // Monday is the first day of the week.
        doy: 4 // The week that contains Jan 4th is the first week of the year.
    }
};
//# sourceMappingURL=da.js.map

/***/ }),

/***/ "./node_modules/ngx-bootstrap/chronos/i18n/de.js":
/*!*******************************************************!*\
  !*** ./node_modules/ngx-bootstrap/chronos/i18n/de.js ***!
  \*******************************************************/
/*! exports provided: deLocale */
/***/ (function(module, __webpack_exports__, __webpack_require__) {

"use strict";
__webpack_require__.r(__webpack_exports__);
/* harmony export (binding) */ __webpack_require__.d(__webpack_exports__, "deLocale", function() { return deLocale; });
// tslint:disable:comment-format binary-expression-operand-order max-line-length
// tslint:disable:no-bitwise prefer-template cyclomatic-complexity
// tslint:disable:no-shadowed-variable switch-default prefer-const
// tslint:disable:one-variable-per-declaration newline-before-return
// tslint:disable:object-literal-key-quotes
//! moment.js locale configuration
//! locale : German [de]
//! author : lluchs : https://github.com/lluchs
//! author: Menelion Elensúle: https://github.com/Oire
//! author : Mikolaj Dadela : https://github.com/mik01aj
function processRelativeTime(num, withoutSuffix, key, isFuture) {
    var format = {
        'm': ['eine Minute', 'einer Minute'],
        'h': ['eine Stunde', 'einer Stunde'],
        'd': ['ein Tag', 'einem Tag'],
        'dd': [num + ' Tage', num + ' Tagen'],
        'M': ['ein Monat', 'einem Monat'],
        'MM': [num + ' Monate', num + ' Monaten'],
        'y': ['ein Jahr', 'einem Jahr'],
        'yy': [num + ' Jahre', num + ' Jahren']
    };
    return withoutSuffix ? format[key][0] : format[key][1];
}
var deLocale = {
    abbr: 'de',
    months: 'Januar_Februar_März_April_Mai_Juni_Juli_August_September_Oktober_November_Dezember'.split('_'),
    monthsShort: 'Jan._Feb._März_Apr._Mai_Juni_Juli_Aug._Sep._Okt._Nov._Dez.'.split('_'),
    monthsParseExact: true,
    weekdays: 'Sonntag_Montag_Dienstag_Mittwoch_Donnerstag_Freitag_Samstag'.split('_'),
    weekdaysShort: 'So._Mo._Di._Mi._Do._Fr._Sa.'.split('_'),
    weekdaysMin: 'So_Mo_Di_Mi_Do_Fr_Sa'.split('_'),
    weekdaysParseExact: true,
    longDateFormat: {
        LT: 'HH:mm',
        LTS: 'HH:mm:ss',
        L: 'DD.MM.YYYY',
        LL: 'D. MMMM YYYY',
        LLL: 'D. MMMM YYYY HH:mm',
        LLLL: 'dddd, D. MMMM YYYY HH:mm'
    },
    calendar: {
        sameDay: '[heute um] LT [Uhr]',
        sameElse: 'L',
        nextDay: '[morgen um] LT [Uhr]',
        nextWeek: 'dddd [um] LT [Uhr]',
        lastDay: '[gestern um] LT [Uhr]',
        lastWeek: '[letzten] dddd [um] LT [Uhr]'
    },
    relativeTime: {
        future: 'in %s',
        past: 'vor %s',
        s: 'ein paar Sekunden',
        ss: '%d Sekunden',
        m: processRelativeTime,
        mm: '%d Minuten',
        h: processRelativeTime,
        hh: '%d Stunden',
        d: processRelativeTime,
        dd: processRelativeTime,
        M: processRelativeTime,
        MM: processRelativeTime,
        y: processRelativeTime,
        yy: processRelativeTime
    },
    dayOfMonthOrdinalParse: /\d{1,2}\./,
    ordinal: '%d.',
    week: {
        dow: 1,
        // Monday is the first day of the week.
        doy: 4 // The week that contains Jan 4th is the first week of the year.
    }
};
//# sourceMappingURL=de.js.map

/***/ }),

/***/ "./node_modules/ngx-bootstrap/chronos/i18n/en-gb.js":
/*!**********************************************************!*\
  !*** ./node_modules/ngx-bootstrap/chronos/i18n/en-gb.js ***!
  \**********************************************************/
/*! exports provided: enGbLocale */
/***/ (function(module, __webpack_exports__, __webpack_require__) {

"use strict";
__webpack_require__.r(__webpack_exports__);
/* harmony export (binding) */ __webpack_require__.d(__webpack_exports__, "enGbLocale", function() { return enGbLocale; });
// tslint:disable:comment-format binary-expression-operand-order max-line-length
// tslint:disable:no-bitwise prefer-template cyclomatic-complexity
// tslint:disable:no-shadowed-variable switch-default prefer-const
// tslint:disable:one-variable-per-declaration newline-before-return
//! moment.js locale configuration
//! locale : English (United Kingdom) [en-gb]
//! author : Chris Gedrim : https://github.com/chrisgedrim
var enGbLocale = {
    abbr: 'en-gb',
    months: 'January_February_March_April_May_June_July_August_September_October_November_December'.split('_'),
    monthsShort: 'Jan_Feb_Mar_Apr_May_Jun_Jul_Aug_Sep_Oct_Nov_Dec'.split('_'),
    weekdays: 'Sunday_Monday_Tuesday_Wednesday_Thursday_Friday_Saturday'.split('_'),
    weekdaysShort: 'Sun_Mon_Tue_Wed_Thu_Fri_Sat'.split('_'),
    weekdaysMin: 'Su_Mo_Tu_We_Th_Fr_Sa'.split('_'),
    longDateFormat: {
        LT: 'HH:mm',
        LTS: 'HH:mm:ss',
        L: 'DD/MM/YYYY',
        LL: 'D MMMM YYYY',
        LLL: 'D MMMM YYYY HH:mm',
        LLLL: 'dddd, D MMMM YYYY HH:mm'
    },
    calendar: {
        sameDay: '[Today at] LT',
        nextDay: '[Tomorrow at] LT',
        nextWeek: 'dddd [at] LT',
        lastDay: '[Yesterday at] LT',
        lastWeek: '[Last] dddd [at] LT',
        sameElse: 'L'
    },
    relativeTime: {
        future: 'in %s',
        past: '%s ago',
        s: 'a few seconds',
        ss: '%d seconds',
        m: 'a minute',
        mm: '%d minutes',
        h: 'an hour',
        hh: '%d hours',
        d: 'a day',
        dd: '%d days',
        M: 'a month',
        MM: '%d months',
        y: 'a year',
        yy: '%d years'
    },
    dayOfMonthOrdinalParse: /\d{1,2}(st|nd|rd|th)/,
    ordinal: function (_num) {
        var num = Number(_num);
        var b = num % 10, output = (~~(num % 100 / 10) === 1) ? 'th' :
            (b === 1) ? 'st' :
                (b === 2) ? 'nd' :
                    (b === 3) ? 'rd' : 'th';
        return num + output;
    },
    week: {
        dow: 1,
        // Monday is the first day of the week.
        doy: 4 // The week that contains Jan 4th is the first week of the year.
    }
};
//# sourceMappingURL=en-gb.js.map

/***/ }),

/***/ "./node_modules/ngx-bootstrap/chronos/i18n/es-do.js":
/*!**********************************************************!*\
  !*** ./node_modules/ngx-bootstrap/chronos/i18n/es-do.js ***!
  \**********************************************************/
/*! exports provided: esDoLocale */
/***/ (function(module, __webpack_exports__, __webpack_require__) {

"use strict";
__webpack_require__.r(__webpack_exports__);
/* harmony export (binding) */ __webpack_require__.d(__webpack_exports__, "esDoLocale", function() { return esDoLocale; });
/* harmony import */ var _utils_date_getters__WEBPACK_IMPORTED_MODULE_0__ = __webpack_require__(/*! ../utils/date-getters */ "./node_modules/ngx-bootstrap/chronos/utils/date-getters.js");
// tslint:disable:comment-format binary-expression-operand-order max-line-length
// tslint:disable:no-bitwise prefer-template cyclomatic-complexity
// tslint:disable:no-shadowed-variable switch-default prefer-const
// tslint:disable:one-variable-per-declaration newline-before-return

//! moment.js locale configuration
//! locale : Spanish (Dominican Republic) [es-do]
var monthsShortDot = 'ene._feb._mar._abr._may._jun._jul._ago._sep._oct._nov._dic.'.split('_'), monthsShort = 'ene_feb_mar_abr_may_jun_jul_ago_sep_oct_nov_dic'.split('_');
var monthsParse = [/^ene/i, /^feb/i, /^mar/i, /^abr/i, /^may/i, /^jun/i, /^jul/i, /^ago/i, /^sep/i, /^oct/i, /^nov/i, /^dic/i];
var monthsRegex = /^(enero|febrero|marzo|abril|mayo|junio|julio|agosto|septiembre|octubre|noviembre|diciembre|ene\.?|feb\.?|mar\.?|abr\.?|may\.?|jun\.?|jul\.?|ago\.?|sep\.?|oct\.?|nov\.?|dic\.?)/i;
var esDoLocale = {
    abbr: 'es-do',
    months: 'enero_febrero_marzo_abril_mayo_junio_julio_agosto_septiembre_octubre_noviembre_diciembre'.split('_'),
    monthsShort: function (date, format, isUTC) {
        if (!date) {
            return monthsShortDot;
        }
        else if (/-MMM-/.test(format)) {
            return monthsShort[Object(_utils_date_getters__WEBPACK_IMPORTED_MODULE_0__["getMonth"])(date, isUTC)];
        }
        else {
            return monthsShortDot[Object(_utils_date_getters__WEBPACK_IMPORTED_MODULE_0__["getMonth"])(date, isUTC)];
        }
    },
    monthsRegex: monthsRegex,
    monthsShortRegex: monthsRegex,
    monthsStrictRegex: /^(enero|febrero|marzo|abril|mayo|junio|julio|agosto|septiembre|octubre|noviembre|diciembre)/i,
    monthsShortStrictRegex: /^(ene\.?|feb\.?|mar\.?|abr\.?|may\.?|jun\.?|jul\.?|ago\.?|sep\.?|oct\.?|nov\.?|dic\.?)/i,
    monthsParse: monthsParse,
    longMonthsParse: monthsParse,
    shortMonthsParse: monthsParse,
    weekdays: 'domingo_lunes_martes_miércoles_jueves_viernes_sábado'.split('_'),
    weekdaysShort: 'dom._lun._mar._mié._jue._vie._sáb.'.split('_'),
    weekdaysMin: 'do_lu_ma_mi_ju_vi_sá'.split('_'),
    weekdaysParseExact: true,
    longDateFormat: {
        LT: 'h:mm A',
        LTS: 'h:mm:ss A',
        L: 'DD/MM/YYYY',
        LL: 'D [de] MMMM [de] YYYY',
        LLL: 'D [de] MMMM [de] YYYY h:mm A',
        LLLL: 'dddd, D [de] MMMM [de] YYYY h:mm A'
    },
    calendar: {
        sameDay: function (date) {
            return '[hoy a la' + ((Object(_utils_date_getters__WEBPACK_IMPORTED_MODULE_0__["getHours"])(date) !== 1) ? 's' : '') + '] LT';
        },
        nextDay: function (date) {
            return '[mañana a la' + ((Object(_utils_date_getters__WEBPACK_IMPORTED_MODULE_0__["getHours"])(date) !== 1) ? 's' : '') + '] LT';
        },
        nextWeek: function (date) {
            return 'dddd [a la' + ((Object(_utils_date_getters__WEBPACK_IMPORTED_MODULE_0__["getHours"])(date) !== 1) ? 's' : '') + '] LT';
        },
        lastDay: function (date) {
            return '[ayer a la' + ((Object(_utils_date_getters__WEBPACK_IMPORTED_MODULE_0__["getHours"])(date) !== 1) ? 's' : '') + '] LT';
        },
        lastWeek: function (date) {
            return '[el] dddd [pasado a la' + ((Object(_utils_date_getters__WEBPACK_IMPORTED_MODULE_0__["getHours"])(date) !== 1) ? 's' : '') + '] LT';
        },
        sameElse: 'L'
    },
    relativeTime: {
        future: 'en %s',
        past: 'hace %s',
        s: 'unos segundos',
        ss: '%d segundos',
        m: 'un minuto',
        mm: '%d minutos',
        h: 'una hora',
        hh: '%d horas',
        d: 'un día',
        dd: '%d días',
        M: 'un mes',
        MM: '%d meses',
        y: 'un año',
        yy: '%d años'
    },
    dayOfMonthOrdinalParse: /\d{1,2}º/,
    ordinal: '%dº',
    week: {
        dow: 1,
        // Monday is the first day of the week.
        doy: 4 // The week that contains Jan 4th is the first week of the year.
    }
};
//# sourceMappingURL=es-do.js.map

/***/ }),

/***/ "./node_modules/ngx-bootstrap/chronos/i18n/es-us.js":
/*!**********************************************************!*\
  !*** ./node_modules/ngx-bootstrap/chronos/i18n/es-us.js ***!
  \**********************************************************/
/*! exports provided: esUsLocale */
/***/ (function(module, __webpack_exports__, __webpack_require__) {

"use strict";
__webpack_require__.r(__webpack_exports__);
/* harmony export (binding) */ __webpack_require__.d(__webpack_exports__, "esUsLocale", function() { return esUsLocale; });
/* harmony import */ var _utils_date_getters__WEBPACK_IMPORTED_MODULE_0__ = __webpack_require__(/*! ../utils/date-getters */ "./node_modules/ngx-bootstrap/chronos/utils/date-getters.js");
// tslint:disable:comment-format binary-expression-operand-order max-line-length
// tslint:disable:no-bitwise prefer-template cyclomatic-complexity
// tslint:disable:no-shadowed-variable switch-default prefer-const
// tslint:disable:one-variable-per-declaration newline-before-return

//! moment.js locale configuration
//! locale : Spanish (United States) [es-us]
//! author : bustta : https://github.com/bustta
var monthsShortDot = 'ene._feb._mar._abr._may._jun._jul._ago._sep._oct._nov._dic.'.split('_');
var monthsShort = 'ene_feb_mar_abr_may_jun_jul_ago_sep_oct_nov_dic'.split('_');
var esUsLocale = {
    abbr: 'es-us',
    months: 'enero_febrero_marzo_abril_mayo_junio_julio_agosto_septiembre_octubre_noviembre_diciembre'.split('_'),
    monthsShort: function (date, format, isUTC) {
        if (!date) {
            return monthsShortDot;
        }
        else if (/-MMM-/.test(format)) {
            return monthsShort[Object(_utils_date_getters__WEBPACK_IMPORTED_MODULE_0__["getMonth"])(date, isUTC)];
        }
        else {
            return monthsShortDot[Object(_utils_date_getters__WEBPACK_IMPORTED_MODULE_0__["getMonth"])(date, isUTC)];
        }
    },
    monthsParseExact: true,
    weekdays: 'domingo_lunes_martes_miércoles_jueves_viernes_sábado'.split('_'),
    weekdaysShort: 'dom._lun._mar._mié._jue._vie._sáb.'.split('_'),
    weekdaysMin: 'do_lu_ma_mi_ju_vi_sá'.split('_'),
    weekdaysParseExact: true,
    longDateFormat: {
        LT: 'h:mm A',
        LTS: 'h:mm:ss A',
        L: 'MM/DD/YYYY',
        LL: 'MMMM [de] D [de] YYYY',
        LLL: 'MMMM [de] D [de] YYYY h:mm A',
        LLLL: 'dddd, MMMM [de] D [de] YYYY h:mm A'
    },
    calendar: {
        sameDay: function (date) {
            return '[hoy a la' + ((Object(_utils_date_getters__WEBPACK_IMPORTED_MODULE_0__["getHours"])(date) !== 1) ? 's' : '') + '] LT';
        },
        nextDay: function (date) {
            return '[mañana a la' + ((Object(_utils_date_getters__WEBPACK_IMPORTED_MODULE_0__["getHours"])(date) !== 1) ? 's' : '') + '] LT';
        },
        nextWeek: function (date) {
            return 'dddd [a la' + ((Object(_utils_date_getters__WEBPACK_IMPORTED_MODULE_0__["getHours"])(date) !== 1) ? 's' : '') + '] LT';
        },
        lastDay: function (date) {
            return '[ayer a la' + ((Object(_utils_date_getters__WEBPACK_IMPORTED_MODULE_0__["getHours"])(date) !== 1) ? 's' : '') + '] LT';
        },
        lastWeek: function (date) {
            return '[el] dddd [pasado a la' + ((Object(_utils_date_getters__WEBPACK_IMPORTED_MODULE_0__["getHours"])(date) !== 1) ? 's' : '') + '] LT';
        },
        sameElse: 'L'
    },
    relativeTime: {
        future: 'en %s',
        past: 'hace %s',
        s: 'unos segundos',
        ss: '%d segundos',
        m: 'un minuto',
        mm: '%d minutos',
        h: 'una hora',
        hh: '%d horas',
        d: 'un día',
        dd: '%d días',
        M: 'un mes',
        MM: '%d meses',
        y: 'un año',
        yy: '%d años'
    },
    dayOfMonthOrdinalParse: /\d{1,2}º/,
    ordinal: '%dº',
    week: {
        dow: 0,
        // Sunday is the first day of the week.
        doy: 6 // The week that contains Jan 1st is the first week of the year.
    }
};
//# sourceMappingURL=es-us.js.map

/***/ }),

/***/ "./node_modules/ngx-bootstrap/chronos/i18n/es.js":
/*!*******************************************************!*\
  !*** ./node_modules/ngx-bootstrap/chronos/i18n/es.js ***!
  \*******************************************************/
/*! exports provided: esLocale */
/***/ (function(module, __webpack_exports__, __webpack_require__) {

"use strict";
__webpack_require__.r(__webpack_exports__);
/* harmony export (binding) */ __webpack_require__.d(__webpack_exports__, "esLocale", function() { return esLocale; });
/* harmony import */ var _utils_date_getters__WEBPACK_IMPORTED_MODULE_0__ = __webpack_require__(/*! ../utils/date-getters */ "./node_modules/ngx-bootstrap/chronos/utils/date-getters.js");
// tslint:disable:comment-format binary-expression-operand-order max-line-length
// tslint:disable:no-bitwise prefer-template cyclomatic-complexity
// tslint:disable:no-shadowed-variable switch-default prefer-const
// tslint:disable:one-variable-per-declaration newline-before-return

//! moment.js locale configuration
//! locale : Spanish [es]
//! author : Julio Napurí : https://github.com/julionc
var monthsShortDot = 'ene._feb._mar._abr._may._jun._jul._ago._sep._oct._nov._dic.'.split('_'), monthsShort = 'ene_feb_mar_abr_may_jun_jul_ago_sep_oct_nov_dic'.split('_');
var monthsParse = [/^ene/i, /^feb/i, /^mar/i, /^abr/i, /^may/i, /^jun/i, /^jul/i, /^ago/i, /^sep/i, /^oct/i, /^nov/i, /^dic/i];
var monthsRegex = /^(enero|febrero|marzo|abril|mayo|junio|julio|agosto|septiembre|octubre|noviembre|diciembre|ene\.?|feb\.?|mar\.?|abr\.?|may\.?|jun\.?|jul\.?|ago\.?|sep\.?|oct\.?|nov\.?|dic\.?)/i;
var esLocale = {
    abbr: 'es',
    months: 'enero_febrero_marzo_abril_mayo_junio_julio_agosto_septiembre_octubre_noviembre_diciembre'.split('_'),
    monthsShort: function (date, format, isUTC) {
        if (!date) {
            return monthsShortDot;
        }
        if (/-MMM-/.test(format)) {
            return monthsShort[Object(_utils_date_getters__WEBPACK_IMPORTED_MODULE_0__["getMonth"])(date, isUTC)];
        }
        return monthsShortDot[Object(_utils_date_getters__WEBPACK_IMPORTED_MODULE_0__["getMonth"])(date, isUTC)];
    },
    monthsRegex: monthsRegex,
    monthsShortRegex: monthsRegex,
    monthsStrictRegex: /^(enero|febrero|marzo|abril|mayo|junio|julio|agosto|septiembre|octubre|noviembre|diciembre)/i,
    monthsShortStrictRegex: /^(ene\.?|feb\.?|mar\.?|abr\.?|may\.?|jun\.?|jul\.?|ago\.?|sep\.?|oct\.?|nov\.?|dic\.?)/i,
    monthsParse: monthsParse,
    longMonthsParse: monthsParse,
    shortMonthsParse: monthsParse,
    weekdays: 'domingo_lunes_martes_miércoles_jueves_viernes_sábado'.split('_'),
    weekdaysShort: 'dom._lun._mar._mié._jue._vie._sáb.'.split('_'),
    weekdaysMin: 'do_lu_ma_mi_ju_vi_sá'.split('_'),
    weekdaysParseExact: true,
    longDateFormat: {
        LT: 'H:mm',
        LTS: 'H:mm:ss',
        L: 'DD/MM/YYYY',
        LL: 'D [de] MMMM [de] YYYY',
        LLL: 'D [de] MMMM [de] YYYY H:mm',
        LLLL: 'dddd, D [de] MMMM [de] YYYY H:mm'
    },
    calendar: {
        sameDay: function (date) {
            return '[hoy a la' + ((Object(_utils_date_getters__WEBPACK_IMPORTED_MODULE_0__["getHours"])(date) !== 1) ? 's' : '') + '] LT';
        },
        nextDay: function (date) {
            return '[mañana a la' + ((Object(_utils_date_getters__WEBPACK_IMPORTED_MODULE_0__["getHours"])(date) !== 1) ? 's' : '') + '] LT';
        },
        nextWeek: function (date) {
            return 'dddd [a la' + ((Object(_utils_date_getters__WEBPACK_IMPORTED_MODULE_0__["getHours"])(date) !== 1) ? 's' : '') + '] LT';
        },
        lastDay: function (date) {
            return '[ayer a la' + ((Object(_utils_date_getters__WEBPACK_IMPORTED_MODULE_0__["getHours"])(date) !== 1) ? 's' : '') + '] LT';
        },
        lastWeek: function (date) {
            return '[el] dddd [pasado a la' + ((Object(_utils_date_getters__WEBPACK_IMPORTED_MODULE_0__["getHours"])(date) !== 1) ? 's' : '') + '] LT';
        },
        sameElse: 'L'
    },
    relativeTime: {
        future: 'en %s',
        past: 'hace %s',
        s: 'unos segundos',
        ss: '%d segundos',
        m: 'un minuto',
        mm: '%d minutos',
        h: 'una hora',
        hh: '%d horas',
        d: 'un día',
        dd: '%d días',
        M: 'un mes',
        MM: '%d meses',
        y: 'un año',
        yy: '%d años'
    },
    dayOfMonthOrdinalParse: /\d{1,2}º/,
    ordinal: '%dº',
    week: {
        dow: 1,
        // Monday is the first day of the week.
        doy: 4 // The week that contains Jan 4th is the first week of the year.
    }
};
//# sourceMappingURL=es.js.map

/***/ }),

/***/ "./node_modules/ngx-bootstrap/chronos/i18n/fi.js":
/*!*******************************************************!*\
  !*** ./node_modules/ngx-bootstrap/chronos/i18n/fi.js ***!
  \*******************************************************/
/*! exports provided: fiLocale */
/***/ (function(module, __webpack_exports__, __webpack_require__) {

"use strict";
__webpack_require__.r(__webpack_exports__);
/* harmony export (binding) */ __webpack_require__.d(__webpack_exports__, "fiLocale", function() { return fiLocale; });
// tslint:disable:comment-format binary-expression-operand-order max-line-length
// tslint:disable:no-bitwise prefer-template cyclomatic-complexity
// tslint:disable:no-shadowed-variable switch-default prefer-const
// tslint:disable:one-variable-per-declaration newline-before-return
//! moment.js locale configuration
// https://github.com/moment/moment/blob/develop/locale/fi.js
var numbersPast = 'nolla yksi kaksi kolme neljä viisi kuusi seitsemän kahdeksan yhdeksän'.split(' '), numbersFuture = [
    'nolla', 'yhden', 'kahden', 'kolmen', 'neljän', 'viiden', 'kuuden',
    numbersPast[7], numbersPast[8], numbersPast[9]
];
function translate(num, withoutSuffix, key, isFuture) {
    var result = '';
    switch (key) {
        case 's':
            return isFuture ? 'muutaman sekunnin' : 'muutama sekunti';
        case 'ss':
            return isFuture ? 'sekunnin' : 'sekuntia';
        case 'm':
            return isFuture ? 'minuutin' : 'minuutti';
        case 'mm':
            result = isFuture ? 'minuutin' : 'minuuttia';
            break;
        case 'h':
            return isFuture ? 'tunnin' : 'tunti';
        case 'hh':
            result = isFuture ? 'tunnin' : 'tuntia';
            break;
        case 'd':
            return isFuture ? 'päivän' : 'päivä';
        case 'dd':
            result = isFuture ? 'päivän' : 'päivää';
            break;
        case 'M':
            return isFuture ? 'kuukauden' : 'kuukausi';
        case 'MM':
            result = isFuture ? 'kuukauden' : 'kuukautta';
            break;
        case 'y':
            return isFuture ? 'vuoden' : 'vuosi';
        case 'yy':
            result = isFuture ? 'vuoden' : 'vuotta';
            break;
    }
    result = verbalNumber(num, isFuture) + ' ' + result;
    return result;
}
function verbalNumber(num, isFuture) {
    return num < 10 ? (isFuture ? numbersFuture[num] : numbersPast[num]) : num;
}
var fiLocale = {
    abbr: 'fi',
    months: 'tammikuu_helmikuu_maaliskuu_huhtikuu_toukokuu_kesäkuu_heinäkuu_elokuu_syyskuu_lokakuu_marraskuu_joulukuu'.split('_'),
    monthsShort: 'tammi_helmi_maalis_huhti_touko_kesä_heinä_elo_syys_loka_marras_joulu'.split('_'),
    weekdays: 'sunnuntai_maanantai_tiistai_keskiviikko_torstai_perjantai_lauantai'.split('_'),
    weekdaysShort: 'su_ma_ti_ke_to_pe_la'.split('_'),
    weekdaysMin: 'su_ma_ti_ke_to_pe_la'.split('_'),
    longDateFormat: {
        LT: 'HH.mm',
        LTS: 'HH.mm.ss',
        L: 'DD.MM.YYYY',
        LL: 'Do MMMM[ta] YYYY',
        LLL: 'Do MMMM[ta] YYYY, [klo] HH.mm',
        LLLL: 'dddd, Do MMMM[ta] YYYY, [klo] HH.mm',
        l: 'D.M.YYYY',
        ll: 'Do MMM YYYY',
        lll: 'Do MMM YYYY, [klo] HH.mm',
        llll: 'ddd, Do MMM YYYY, [klo] HH.mm'
    },
    calendar: {
        sameDay: '[tänään] [klo] LT',
        nextDay: '[huomenna] [klo] LT',
        nextWeek: 'dddd [klo] LT',
        lastDay: '[eilen] [klo] LT',
        lastWeek: '[viime] dddd[na] [klo] LT',
        sameElse: 'L'
    },
    relativeTime: {
        future: '%s päästä',
        past: '%s sitten',
        s: translate,
        ss: translate,
        m: translate,
        mm: translate,
        h: translate,
        hh: translate,
        d: translate,
        dd: translate,
        M: translate,
        MM: translate,
        y: translate,
        yy: translate
    },
    dayOfMonthOrdinalParse: /\d{1,2}\./,
    ordinal: '%d.',
    week: {
        dow: 1,
        // Monday is the first day of the week.
        doy: 4 // The week that contains Jan 4th is the first week of the year.
    }
};
//# sourceMappingURL=fi.js.map

/***/ }),

/***/ "./node_modules/ngx-bootstrap/chronos/i18n/fr.js":
/*!*******************************************************!*\
  !*** ./node_modules/ngx-bootstrap/chronos/i18n/fr.js ***!
  \*******************************************************/
/*! exports provided: frLocale */
/***/ (function(module, __webpack_exports__, __webpack_require__) {

"use strict";
__webpack_require__.r(__webpack_exports__);
/* harmony export (binding) */ __webpack_require__.d(__webpack_exports__, "frLocale", function() { return frLocale; });
// tslint:disable:comment-format binary-expression-operand-order max-line-length
// tslint:disable:no-bitwise prefer-template cyclomatic-complexity
// tslint:disable:no-shadowed-variable switch-default prefer-const
// tslint:disable:one-variable-per-declaration newline-before-return
//! moment.js locale configuration
//! locale : French [fr]
//! author : John Fischer : https://github.com/jfroffice
var frLocale = {
    abbr: 'fr',
    months: 'janvier_février_mars_avril_mai_juin_juillet_août_septembre_octobre_novembre_décembre'.split('_'),
    monthsShort: 'janv._févr._mars_avr._mai_juin_juil._août_sept._oct._nov._déc.'.split('_'),
    monthsParseExact: true,
    weekdays: 'dimanche_lundi_mardi_mercredi_jeudi_vendredi_samedi'.split('_'),
    weekdaysShort: 'dim._lun._mar._mer._jeu._ven._sam.'.split('_'),
    weekdaysMin: 'di_lu_ma_me_je_ve_sa'.split('_'),
    weekdaysParseExact: true,
    longDateFormat: {
        LT: 'HH:mm',
        LTS: 'HH:mm:ss',
        L: 'DD/MM/YYYY',
        LL: 'D MMMM YYYY',
        LLL: 'D MMMM YYYY HH:mm',
        LLLL: 'dddd D MMMM YYYY HH:mm'
    },
    calendar: {
        sameDay: '[Aujourd’hui à] LT',
        nextDay: '[Demain à] LT',
        nextWeek: 'dddd [à] LT',
        lastDay: '[Hier à] LT',
        lastWeek: 'dddd [dernier à] LT',
        sameElse: 'L'
    },
    relativeTime: {
        future: 'dans %s',
        past: 'il y a %s',
        s: 'quelques secondes',
        ss: '%d secondes',
        m: 'une minute',
        mm: '%d minutes',
        h: 'une heure',
        hh: '%d heures',
        d: 'un jour',
        dd: '%d jours',
        M: 'un mois',
        MM: '%d mois',
        y: 'un an',
        yy: '%d ans'
    },
    dayOfMonthOrdinalParse: /\d{1,2}(er|)/,
    ordinal: function (_num, period) {
        var num = Number(_num);
        switch (period) {
            // TODO: Return 'e' when day of month > 1. Move this case inside
            // block for masculine words below.
            // See https://github.com/moment/moment/issues/3375
            case 'D':
                return num + (num === 1 ? 'er' : '');
            // Words with masculine grammatical gender: mois, trimestre, jour
            default:
            case 'M':
            case 'Q':
            case 'DDD':
            case 'd':
                return num + (num === 1 ? 'er' : 'e');
            // Words with feminine grammatical gender: semaine
            case 'w':
            case 'W':
                return num + (num === 1 ? 're' : 'e');
        }
    },
    week: {
        dow: 1,
        // Monday is the first day of the week.
        doy: 4 // The week that contains Jan 4th is the first week of the year.
    }
};
//# sourceMappingURL=fr.js.map

/***/ }),

/***/ "./node_modules/ngx-bootstrap/chronos/i18n/gl.js":
/*!*******************************************************!*\
  !*** ./node_modules/ngx-bootstrap/chronos/i18n/gl.js ***!
  \*******************************************************/
/*! exports provided: glLocale */
/***/ (function(module, __webpack_exports__, __webpack_require__) {

"use strict";
__webpack_require__.r(__webpack_exports__);
/* harmony export (binding) */ __webpack_require__.d(__webpack_exports__, "glLocale", function() { return glLocale; });
/* harmony import */ var _utils_date_getters__WEBPACK_IMPORTED_MODULE_0__ = __webpack_require__(/*! ../utils/date-getters */ "./node_modules/ngx-bootstrap/chronos/utils/date-getters.js");
// tslint:disable:comment-format binary-expression-operand-order max-line-length
// tslint:disable:no-bitwise prefer-template cyclomatic-complexity
// tslint:disable:no-shadowed-variable switch-default prefer-const
// tslint:disable:one-variable-per-declaration newline-before-return

//! moment.js locale configuration
//! locale : Galician [gl]
//! author : Darío Beiró : https://github.com/quinobravo
var monthsShortDot = 'xan._feb._mar._abr._mai._xuñ._xul._ago._set._out._nov._dec.'.split('_'), monthsShort = 'xan_feb_mar_abr_mai_xuñ_xul_ago_set_out_nov_dec'.split('_');
var monthsParse = [/^xan/i, /^feb/i, /^mar/i, /^abr/i, /^mai/i, /^xuñ/i, /^xul/i, /^ago/i, /^set/i, /^out/i, /^nov/i, /^dec/i];
var monthsRegex = /^(xaneiro|febreiro|marzo|abril|maio|xuño|xullo|agosto|setembro|outubro|novembro|decembro|xan\.?|feb\.?|mar\.?|abr\.?|mai\.?|xuñ\.?|xul\.?|ago\.?|set\.?|out\.?|nov\.?|dec\.?)/i;
var glLocale = {
    abbr: 'gl',
    months: 'xaneiro_febreiro_marzo_abril_maio_xuño_xullo_agosto_setembro_outubro_novembro_decembro'.split('_'),
    monthsShort: function (date, format, isUTC) {
        if (!date) {
            return monthsShortDot;
        }
        if (/-MMM-/.test(format)) {
            return monthsShort[Object(_utils_date_getters__WEBPACK_IMPORTED_MODULE_0__["getMonth"])(date, isUTC)];
        }
        return monthsShortDot[Object(_utils_date_getters__WEBPACK_IMPORTED_MODULE_0__["getMonth"])(date, isUTC)];
    },
    monthsRegex: monthsRegex,
    monthsShortRegex: monthsRegex,
    monthsStrictRegex: /^(xaneiro|febreiro|marzo|abril|maio|xuño|xullo|agosto|setembro|outubro|novembro|decembro)/i,
    monthsShortStrictRegex: /^(xan\.?|feb\.?|mar\.?|abr\.?|mai\.?|xuñ\.?|xul\.?|ago\.?|set\.?|out\.?|nov\.?|dec\.?)/i,
    monthsParse: monthsParse,
    longMonthsParse: monthsParse,
    shortMonthsParse: monthsParse,
    weekdays: 'domingo_luns_martes_mércores_xoves_venres_sábado'.split('_'),
    weekdaysShort: 'dom._lun._mar._mér._xov._ven._sáb.'.split('_'),
    weekdaysMin: 'do_lu_ma_mé_xo_ve_sá'.split('_'),
    weekdaysParseExact: true,
    longDateFormat: {
        LT: 'H:mm',
        LTS: 'H:mm:ss',
        L: 'DD/MM/YYYY',
        LL: 'D [de] MMMM [de] YYYY',
        LLL: 'D [de] MMMM [de] YYYY H:mm',
        LLLL: 'dddd, D [de] MMMM [de] YYYY H:mm'
    },
    calendar: {
        sameDay: function (date) {
            return '[hoxe á' + ((Object(_utils_date_getters__WEBPACK_IMPORTED_MODULE_0__["getHours"])(date) !== 1) ? 's' : '') + '] LT';
        },
        nextDay: function (date) {
            return '[mañan á' + ((Object(_utils_date_getters__WEBPACK_IMPORTED_MODULE_0__["getHours"])(date) !== 1) ? 's' : '') + '] LT';
        },
        nextWeek: function (date) {
            return 'dddd [á' + ((Object(_utils_date_getters__WEBPACK_IMPORTED_MODULE_0__["getHours"])(date) !== 1) ? 's' : '') + '] LT';
        },
        lastDay: function (date) {
            return '[onte á' + ((Object(_utils_date_getters__WEBPACK_IMPORTED_MODULE_0__["getHours"])(date) !== 1) ? 's' : '') + '] LT';
        },
        lastWeek: function (date) {
            return '[o] dddd [pasado á' + ((Object(_utils_date_getters__WEBPACK_IMPORTED_MODULE_0__["getHours"])(date) !== 1) ? 's' : '') + '] LT';
        },
        sameElse: 'L'
    },
    relativeTime: {
        future: 'en %s',
        past: 'fai %s',
        s: 'uns segundos',
        ss: '%d segundos',
        m: 'un minuto',
        mm: '%d minutos',
        h: 'unha hora',
        hh: '%d horas',
        d: 'un día',
        dd: '%d días',
        M: 'un mes',
        MM: '%d meses',
        y: 'un ano',
        yy: '%d anos'
    },
    dayOfMonthOrdinalParse: /\d{1,2}º/,
    ordinal: '%dº',
    week: {
        dow: 1,
        // Monday is the first day of the week.
        doy: 4 // The week that contains Jan 4th is the first week of the year.
    }
};
//# sourceMappingURL=gl.js.map

/***/ }),

/***/ "./node_modules/ngx-bootstrap/chronos/i18n/he.js":
/*!*******************************************************!*\
  !*** ./node_modules/ngx-bootstrap/chronos/i18n/he.js ***!
  \*******************************************************/
/*! exports provided: heLocale */
/***/ (function(module, __webpack_exports__, __webpack_require__) {

"use strict";
__webpack_require__.r(__webpack_exports__);
/* harmony export (binding) */ __webpack_require__.d(__webpack_exports__, "heLocale", function() { return heLocale; });
// tslint:disable:comment-format binary-expression-operand-order max-line-length
// tslint:disable:no-bitwise prefer-template cyclomatic-complexity
// tslint:disable:no-shadowed-variable switch-default prefer-const
// tslint:disable:one-variable-per-declaration newline-before-return
//! moment.js locale configuration
//! locale : Hebrew [he]
//! author : Tomer Cohen : https://github.com/tomer
//! author : Moshe Simantov : https://github.com/DevelopmentIL
//! author : Tal Ater : https://github.com/TalAter
var heLocale = {
    abbr: 'he',
    months: 'ינואר_פברואר_מרץ_אפריל_מאי_יוני_יולי_אוגוסט_ספטמבר_אוקטובר_נובמבר_דצמבר'.split('_'),
    monthsShort: 'ינו׳_פבר׳_מרץ_אפר׳_מאי_יוני_יולי_אוג׳_ספט׳_אוק׳_נוב׳_דצמ׳'.split('_'),
    weekdays: 'ראשון_שני_שלישי_רביעי_חמישי_שישי_שבת'.split('_'),
    weekdaysShort: 'א׳_ב׳_ג׳_ד׳_ה׳_ו׳_ש׳'.split('_'),
    weekdaysMin: 'א_ב_ג_ד_ה_ו_ש'.split('_'),
    longDateFormat: {
        LT: 'HH:mm',
        LTS: 'HH:mm:ss',
        L: 'DD/MM/YYYY',
        LL: 'D [ב]MMMM YYYY',
        LLL: 'D [ב]MMMM YYYY HH:mm',
        LLLL: 'dddd, D [ב]MMMM YYYY HH:mm',
        l: 'D/M/YYYY',
        ll: 'D MMM YYYY',
        lll: 'D MMM YYYY HH:mm',
        llll: 'ddd, D MMM YYYY HH:mm'
    },
    calendar: {
        sameDay: '[היום ב־]LT',
        nextDay: '[מחר ב־]LT',
        nextWeek: 'dddd [בשעה] LT',
        lastDay: '[אתמול ב־]LT',
        lastWeek: '[ביום] dddd [האחרון בשעה] LT',
        sameElse: 'L'
    },
    relativeTime: {
        future: 'בעוד %s',
        past: 'לפני %s',
        s: 'מספר שניות',
        ss: '%d שניות',
        m: 'דקה',
        mm: '%d דקות',
        h: 'שעה',
        hh: function (num) {
            if (num === 2) {
                return 'שעתיים';
            }
            return num + ' שעות';
        },
        d: 'יום',
        dd: function (num) {
            if (num === 2) {
                return 'יומיים';
            }
            return num + ' ימים';
        },
        M: 'חודש',
        MM: function (num) {
            if (num === 2) {
                return 'חודשיים';
            }
            return num + ' חודשים';
        },
        y: 'שנה',
        yy: function (num) {
            if (num === 2) {
                return 'שנתיים';
            }
            else if (num % 10 === 0 && num !== 10) {
                return num + ' שנה';
            }
            return num + ' שנים';
        }
    },
    meridiemParse: /אחה"צ|לפנה"צ|אחרי הצהריים|לפני הצהריים|לפנות בוקר|בבוקר|בערב/i,
    isPM: function (input) {
        return /^(אחה"צ|אחרי הצהריים|בערב)$/.test(input);
    },
    meridiem: function (hour, minute, isLower) {
        if (hour < 5) {
            return 'לפנות בוקר';
        }
        else if (hour < 10) {
            return 'בבוקר';
        }
        else if (hour < 12) {
            return isLower ? 'לפנה"צ' : 'לפני הצהריים';
        }
        else if (hour < 18) {
            return isLower ? 'אחה"צ' : 'אחרי הצהריים';
        }
        else {
            return 'בערב';
        }
    }
};
//# sourceMappingURL=he.js.map

/***/ }),

/***/ "./node_modules/ngx-bootstrap/chronos/i18n/hi.js":
/*!*******************************************************!*\
  !*** ./node_modules/ngx-bootstrap/chronos/i18n/hi.js ***!
  \*******************************************************/
/*! exports provided: hiLocale */
/***/ (function(module, __webpack_exports__, __webpack_require__) {

"use strict";
__webpack_require__.r(__webpack_exports__);
/* harmony export (binding) */ __webpack_require__.d(__webpack_exports__, "hiLocale", function() { return hiLocale; });
// tslint:disable:comment-format binary-expression-operand-order max-line-length
// tslint:disable:no-bitwise prefer-template cyclomatic-complexity
// tslint:disable:no-shadowed-variable switch-default prefer-const
// tslint:disable:one-variable-per-declaration newline-before-return
// tslint:disable:no-parameter-reassignment prefer-switch
//! moment.js locale configuration
//! locale : Hindi [hi]
//! author : Mayank Singhal : https://github.com/mayanksinghal
var symbolMap = {
    1: '१',
    2: '२',
    3: '३',
    4: '४',
    5: '५',
    6: '६',
    7: '७',
    8: '८',
    9: '९',
    0: '०'
}, numberMap = {
    '१': '1',
    '२': '2',
    '३': '3',
    '४': '4',
    '५': '5',
    '६': '6',
    '७': '7',
    '८': '8',
    '९': '9',
    '०': '0'
};
var hiLocale = {
    abbr: 'hi',
    months: 'जनवरी_फ़रवरी_मार्च_अप्रैल_मई_जून_जुलाई_अगस्त_सितम्बर_अक्टूबर_नवम्बर_दिसम्बर'.split('_'),
    monthsShort: 'जन._फ़र._मार्च_अप्रै._मई_जून_जुल._अग._सित._अक्टू._नव._दिस.'.split('_'),
    monthsParseExact: true,
    weekdays: 'रविवार_सोमवार_मंगलवार_बुधवार_गुरूवार_शुक्रवार_शनिवार'.split('_'),
    weekdaysShort: 'रवि_सोम_मंगल_बुध_गुरू_शुक्र_शनि'.split('_'),
    weekdaysMin: 'र_सो_मं_बु_गु_शु_श'.split('_'),
    longDateFormat: {
        LT: 'A h:mm बजे',
        LTS: 'A h:mm:ss बजे',
        L: 'DD/MM/YYYY',
        LL: 'D MMMM YYYY',
        LLL: 'D MMMM YYYY, A h:mm बजे',
        LLLL: 'dddd, D MMMM YYYY, A h:mm बजे'
    },
    calendar: {
        sameDay: '[आज] LT',
        nextDay: '[कल] LT',
        nextWeek: 'dddd, LT',
        lastDay: '[कल] LT',
        lastWeek: '[पिछले] dddd, LT',
        sameElse: 'L'
    },
    relativeTime: {
        future: '%s में',
        past: '%s पहले',
        s: 'कुछ ही क्षण',
        ss: '%d सेकंड',
        m: 'एक मिनट',
        mm: '%d मिनट',
        h: 'एक घंटा',
        hh: '%d घंटे',
        d: 'एक दिन',
        dd: '%d दिन',
        M: 'एक महीने',
        MM: '%d महीने',
        y: 'एक वर्ष',
        yy: '%d वर्ष'
    },
    preparse: function (str) {
        return str.replace(/[१२३४५६७८९०]/g, function (match) {
            return numberMap[match];
        });
    },
    postformat: function (str) {
        return str.replace(/\d/g, function (match) {
            return symbolMap[match];
        });
    },
    // Hindi notation for meridiems are quite fuzzy in practice. While there exists
    // a rigid notion of a 'Pahar' it is not used as rigidly in modern Hindi.
    meridiemParse: /रात|सुबह|दोपहर|शाम/,
    meridiemHour: function (hour, meridiem) {
        if (hour === 12) {
            hour = 0;
        }
        if (meridiem === 'रात') {
            return hour < 4 ? hour : hour + 12;
        }
        else if (meridiem === 'सुबह') {
            return hour;
        }
        else if (meridiem === 'दोपहर') {
            return hour >= 10 ? hour : hour + 12;
        }
        else if (meridiem === 'शाम') {
            return hour + 12;
        }
    },
    meridiem: function (hour, minute, isLower) {
        if (hour < 4) {
            return 'रात';
        }
        else if (hour < 10) {
            return 'सुबह';
        }
        else if (hour < 17) {
            return 'दोपहर';
        }
        else if (hour < 20) {
            return 'शाम';
        }
        else {
            return 'रात';
        }
    },
    week: {
        dow: 0,
        // Sunday is the first day of the week.
        doy: 6 // The week that contains Jan 1st is the first week of the year.
    }
};
//# sourceMappingURL=hi.js.map

/***/ }),

/***/ "./node_modules/ngx-bootstrap/chronos/i18n/hu.js":
/*!*******************************************************!*\
  !*** ./node_modules/ngx-bootstrap/chronos/i18n/hu.js ***!
  \*******************************************************/
/*! exports provided: huLocale */
/***/ (function(module, __webpack_exports__, __webpack_require__) {

"use strict";
__webpack_require__.r(__webpack_exports__);
/* harmony export (binding) */ __webpack_require__.d(__webpack_exports__, "huLocale", function() { return huLocale; });
/* harmony import */ var _units_day_of_week__WEBPACK_IMPORTED_MODULE_0__ = __webpack_require__(/*! ../units/day-of-week */ "./node_modules/ngx-bootstrap/chronos/units/day-of-week.js");
// tslint:disable:comment-format binary-expression-operand-order max-line-length
// tslint:disable:no-bitwise prefer-template cyclomatic-complexity
// tslint:disable:no-shadowed-variable switch-default prefer-const
// tslint:disable:one-variable-per-declaration newline-before-return

//! moment.js locale configuration
//! locale : Hungarian [hu]
//! author : Adam Brunner : https://github.com/adambrunner
var weekEndings = 'vasárnap hétfőn kedden szerdán csütörtökön pénteken szombaton'.split(' ');
function translate(num, withoutSuffix, key, isFuture) {
    switch (key) {
        case 's':
            return (isFuture || withoutSuffix) ? 'néhány másodperc' : 'néhány másodperce';
        case 'ss':
            return num + ((isFuture || withoutSuffix) ? ' másodperc' : ' másodperce');
        case 'm':
            return 'egy' + (isFuture || withoutSuffix ? ' perc' : ' perce');
        case 'mm':
            return num + (isFuture || withoutSuffix ? ' perc' : ' perce');
        case 'h':
            return 'egy' + (isFuture || withoutSuffix ? ' óra' : ' órája');
        case 'hh':
            return num + (isFuture || withoutSuffix ? ' óra' : ' órája');
        case 'd':
            return 'egy' + (isFuture || withoutSuffix ? ' nap' : ' napja');
        case 'dd':
            return num + (isFuture || withoutSuffix ? ' nap' : ' napja');
        case 'M':
            return 'egy' + (isFuture || withoutSuffix ? ' hónap' : ' hónapja');
        case 'MM':
            return num + (isFuture || withoutSuffix ? ' hónap' : ' hónapja');
        case 'y':
            return 'egy' + (isFuture || withoutSuffix ? ' év' : ' éve');
        case 'yy':
            return num + (isFuture || withoutSuffix ? ' év' : ' éve');
    }
    return '';
}
function week(date, isFuture) {
    return (isFuture ? '' : '[múlt] ') + '[' + weekEndings[Object(_units_day_of_week__WEBPACK_IMPORTED_MODULE_0__["getDayOfWeek"])(date)] + '] LT[-kor]';
}
var huLocale = {
    abbr: 'hu',
    months: 'január_február_március_április_május_június_július_augusztus_szeptember_október_november_december'.split('_'),
    monthsShort: 'jan_feb_márc_ápr_máj_jún_júl_aug_szept_okt_nov_dec'.split('_'),
    weekdays: 'vasárnap_hétfő_kedd_szerda_csütörtök_péntek_szombat'.split('_'),
    weekdaysShort: 'vas_hét_kedd_sze_csüt_pén_szo'.split('_'),
    weekdaysMin: 'v_h_k_sze_cs_p_szo'.split('_'),
    longDateFormat: {
        LT: 'H:mm',
        LTS: 'H:mm:ss',
        L: 'YYYY.MM.DD.',
        LL: 'YYYY. MMMM D.',
        LLL: 'YYYY. MMMM D. H:mm',
        LLLL: 'YYYY. MMMM D., dddd H:mm'
    },
    meridiemParse: /de|du/i,
    isPM: function (input) {
        return input.charAt(1).toLowerCase() === 'u';
    },
    meridiem: function (hours, minutes, isLower) {
        if (hours < 12) {
            return isLower === true ? 'de' : 'DE';
        }
        else {
            return isLower === true ? 'du' : 'DU';
        }
    },
    calendar: {
        sameDay: '[ma] LT[-kor]',
        nextDay: '[holnap] LT[-kor]',
        nextWeek: function (date) {
            return week(date, true);
        },
        lastDay: '[tegnap] LT[-kor]',
        lastWeek: function (date) {
            return week(date, false);
        },
        sameElse: 'L'
    },
    relativeTime: {
        future: '%s múlva',
        past: '%s',
        s: translate,
        ss: translate,
        m: translate,
        mm: translate,
        h: translate,
        hh: translate,
        d: translate,
        dd: translate,
        M: translate,
        MM: translate,
        y: translate,
        yy: translate
    },
    dayOfMonthOrdinalParse: /\d{1,2}\./,
    ordinal: '%d.',
    week: {
        dow: 1,
        // Monday is the first day of the week.
        doy: 4 // The week that contains Jan 4th is the first week of the year.
    }
};
//# sourceMappingURL=hu.js.map

/***/ }),

/***/ "./node_modules/ngx-bootstrap/chronos/i18n/id.js":
/*!*******************************************************!*\
  !*** ./node_modules/ngx-bootstrap/chronos/i18n/id.js ***!
  \*******************************************************/
/*! exports provided: idLocale */
/***/ (function(module, __webpack_exports__, __webpack_require__) {

"use strict";
__webpack_require__.r(__webpack_exports__);
/* harmony export (binding) */ __webpack_require__.d(__webpack_exports__, "idLocale", function() { return idLocale; });
// tslint:disable:comment-format binary-expression-operand-order max-line-length
// tslint:disable:no-bitwise prefer-template cyclomatic-complexity
// tslint:disable:no-shadowed-variable switch-default prefer-const
// tslint:disable:one-variable-per-declaration newline-before-return
// tslint:disable:no-parameter-reassignment prefer-switch
//! moment.js locale configuration
//! locale : Indonesia [id]
//! author : Romy Kusuma : https://github.com/rkusuma
//! reference: https://github.com/moment/moment/blob/develop/locale/id.js
var idLocale = {
    abbr: 'id',
    months: 'Januari_Februari_Maret_April_Mei_Juni_Juli_Agustus_September_Oktober_November_Desember'.split('_'),
    monthsShort: 'Jan_Feb_Mar_Apr_Mei_Jun_Jul_Ags_Sep_Okt_Nov_Des'.split('_'),
    weekdays: 'Minggu_Senin_Selasa_Rabu_Kamis_Jumat_Sabtu'.split('_'),
    weekdaysShort: 'Min_Sen_Sel_Rab_Kam_Jum_Sab'.split('_'),
    weekdaysMin: 'Mg_Sn_Sl_Rb_Km_Jm_Sb'.split('_'),
    longDateFormat: {
        LT: 'HH.mm',
        LTS: 'HH.mm.ss',
        L: 'DD/MM/YYYY',
        LL: 'D MMMM YYYY',
        LLL: 'D MMMM YYYY [pukul] HH.mm',
        LLLL: 'dddd, D MMMM YYYY [pukul] HH.mm'
    },
    meridiemParse: /pagi|siang|sore|malam/,
    meridiemHour: function (hour, meridiem) {
        if (hour === 12) {
            hour = 0;
        }
        if (meridiem === 'pagi') {
            return hour;
        }
        else if (meridiem === 'siang') {
            return hour >= 11 ? hour : hour + 12;
        }
        else if (meridiem === 'sore' || meridiem === 'malam') {
            return hour + 12;
        }
    },
    meridiem: function (hours, minutes, isLower) {
        if (hours < 11) {
            return 'pagi';
        }
        else if (hours < 15) {
            return 'siang';
        }
        else if (hours < 19) {
            return 'sore';
        }
        else {
            return 'malam';
        }
    },
    calendar: {
        sameDay: '[Hari ini pukul] LT',
        nextDay: '[Besok pukul] LT',
        nextWeek: 'dddd [pukul] LT',
        lastDay: '[Kemarin pukul] LT',
        lastWeek: 'dddd [lalu pukul] LT',
        sameElse: 'L'
    },
    relativeTime: {
        future: 'dalam %s',
        past: '%s yang lalu',
        s: 'beberapa detik',
        ss: '%d detik',
        m: 'semenit',
        mm: '%d menit',
        h: 'sejam',
        hh: '%d jam',
        d: 'sehari',
        dd: '%d hari',
        M: 'sebulan',
        MM: '%d bulan',
        y: 'setahun',
        yy: '%d tahun'
    },
    week: {
        dow: 1,
        // Monday is the first day of the week.
        doy: 7 // The week that contains Jan 1st is the first week of the year.
    }
};
//# sourceMappingURL=id.js.map

/***/ }),

/***/ "./node_modules/ngx-bootstrap/chronos/i18n/it.js":
/*!*******************************************************!*\
  !*** ./node_modules/ngx-bootstrap/chronos/i18n/it.js ***!
  \*******************************************************/
/*! exports provided: itLocale */
/***/ (function(module, __webpack_exports__, __webpack_require__) {

"use strict";
__webpack_require__.r(__webpack_exports__);
/* harmony export (binding) */ __webpack_require__.d(__webpack_exports__, "itLocale", function() { return itLocale; });
/* harmony import */ var _units_day_of_week__WEBPACK_IMPORTED_MODULE_0__ = __webpack_require__(/*! ../units/day-of-week */ "./node_modules/ngx-bootstrap/chronos/units/day-of-week.js");
// tslint:disable:comment-format binary-expression-operand-order max-line-length
// tslint:disable:no-bitwise prefer-template cyclomatic-complexity
// tslint:disable:no-shadowed-variable switch-default prefer-const
// tslint:disable:one-variable-per-declaration newline-before-return

//! moment.js locale configuration
//! locale : Italian [it]
//! author : Lorenzo : https://github.com/aliem
//! author: Mattia Larentis: https://github.com/nostalgiaz
var itLocale = {
    abbr: 'it',
    months: 'gennaio_febbraio_marzo_aprile_maggio_giugno_luglio_agosto_settembre_ottobre_novembre_dicembre'.split('_'),
    monthsShort: 'gen_feb_mar_apr_mag_giu_lug_ago_set_ott_nov_dic'.split('_'),
    weekdays: 'domenica_lunedì_martedì_mercoledì_giovedì_venerdì_sabato'.split('_'),
    weekdaysShort: 'dom_lun_mar_mer_gio_ven_sab'.split('_'),
    weekdaysMin: 'do_lu_ma_me_gi_ve_sa'.split('_'),
    longDateFormat: {
        LT: 'HH:mm',
        LTS: 'HH:mm:ss',
        L: 'DD/MM/YYYY',
        LL: 'D MMMM YYYY',
        LLL: 'D MMMM YYYY HH:mm',
        LLLL: 'dddd D MMMM YYYY HH:mm'
    },
    calendar: {
        sameDay: '[Oggi alle] LT',
        nextDay: '[Domani alle] LT',
        nextWeek: 'dddd [alle] LT',
        lastDay: '[Ieri alle] LT',
        lastWeek: function (date) {
            switch (Object(_units_day_of_week__WEBPACK_IMPORTED_MODULE_0__["getDayOfWeek"])(date)) {
                case 0:
                    return '[la scorsa] dddd [alle] LT';
                default:
                    return '[lo scorso] dddd [alle] LT';
            }
        },
        sameElse: 'L'
    },
    relativeTime: {
        future: function (num) {
            return ((/^[0-9].+$/).test(num.toString(10)) ? 'tra' : 'in') + ' ' + num;
        },
        past: '%s fa',
        s: 'alcuni secondi',
        ss: '%d secondi',
        m: 'un minuto',
        mm: '%d minuti',
        h: 'un\'ora',
        hh: '%d ore',
        d: 'un giorno',
        dd: '%d giorni',
        M: 'un mese',
        MM: '%d mesi',
        y: 'un anno',
        yy: '%d anni'
    },
    dayOfMonthOrdinalParse: /\d{1,2}º/,
    ordinal: '%dº',
    week: {
        dow: 1,
        // Monday is the first day of the week.
        doy: 4 // The week that contains Jan 4th is the first week of the year.
    }
};
//# sourceMappingURL=it.js.map

/***/ }),

/***/ "./node_modules/ngx-bootstrap/chronos/i18n/ja.js":
/*!*******************************************************!*\
  !*** ./node_modules/ngx-bootstrap/chronos/i18n/ja.js ***!
  \*******************************************************/
/*! exports provided: jaLocale */
/***/ (function(module, __webpack_exports__, __webpack_require__) {

"use strict";
__webpack_require__.r(__webpack_exports__);
/* harmony export (binding) */ __webpack_require__.d(__webpack_exports__, "jaLocale", function() { return jaLocale; });
// tslint:disable:comment-format binary-expression-operand-order max-line-length
// tslint:disable:no-bitwise prefer-template cyclomatic-complexity
// tslint:disable:no-shadowed-variable switch-default prefer-const
// tslint:disable:one-variable-per-declaration newline-before-return
//! moment.js locale configuration
//! locale : Japanese [ja]
//! author : LI Long : https://github.com/baryon
var jaLocale = {
    abbr: 'ja',
    months: '1月_2月_3月_4月_5月_6月_7月_8月_9月_10月_11月_12月'.split('_'),
    monthsShort: '1月_2月_3月_4月_5月_6月_7月_8月_9月_10月_11月_12月'.split('_'),
    weekdays: '日曜日_月曜日_火曜日_水曜日_木曜日_金曜日_土曜日'.split('_'),
    weekdaysShort: '日_月_火_水_木_金_土'.split('_'),
    weekdaysMin: '日_月_火_水_木_金_土'.split('_'),
    longDateFormat: {
        LT: 'HH:mm',
        LTS: 'HH:mm:ss',
        L: 'YYYY/MM/DD',
        LL: 'YYYY年M月D日',
        LLL: 'YYYY年M月D日 HH:mm',
        LLLL: 'YYYY年M月D日 HH:mm dddd',
        l: 'YYYY/MM/DD',
        ll: 'YYYY年M月D日',
        lll: 'YYYY年M月D日 HH:mm',
        llll: 'YYYY年M月D日 HH:mm dddd'
    },
    meridiemParse: /午前|午後/i,
    isPM: function (input) {
        return input === '午後';
    },
    meridiem: function (hour, minute, isLower) {
        if (hour < 12) {
            return '午前';
        }
        else {
            return '午後';
        }
    },
    calendar: {
        sameDay: '[今日] LT',
        nextDay: '[明日] LT',
        nextWeek: '[来週]dddd LT',
        lastDay: '[昨日] LT',
        lastWeek: '[前週]dddd LT',
        sameElse: 'L'
    },
    dayOfMonthOrdinalParse: /\d{1,2}日/,
    ordinal: function (num, period) {
        switch (period) {
            case 'd':
            case 'D':
            case 'DDD':
                return num + '日';
            default:
                return num.toString(10);
        }
    },
    relativeTime: {
        future: '%s後',
        past: '%s前',
        s: '数秒',
        ss: '%d秒',
        m: '1分',
        mm: '%d分',
        h: '1時間',
        hh: '%d時間',
        d: '1日',
        dd: '%d日',
        M: '1ヶ月',
        MM: '%dヶ月',
        y: '1年',
        yy: '%d年'
    }
};
//# sourceMappingURL=ja.js.map

/***/ }),

/***/ "./node_modules/ngx-bootstrap/chronos/i18n/ko.js":
/*!*******************************************************!*\
  !*** ./node_modules/ngx-bootstrap/chronos/i18n/ko.js ***!
  \*******************************************************/
/*! exports provided: koLocale */
/***/ (function(module, __webpack_exports__, __webpack_require__) {

"use strict";
__webpack_require__.r(__webpack_exports__);
/* harmony export (binding) */ __webpack_require__.d(__webpack_exports__, "koLocale", function() { return koLocale; });
// tslint:disable:comment-format binary-expression-operand-order max-line-length
// tslint:disable:no-bitwise prefer-template cyclomatic-complexity
// tslint:disable:no-shadowed-variable switch-default prefer-const
// tslint:disable:one-variable-per-declaration newline-before-return
// tslint:disable:object-literal-shorthand
//! moment.js locale configuration
//! locale : Korean [ko]
//! author : Kyungwook, Park : https://github.com/kyungw00k
//! author : Jeeeyul Lee <jeeeyul@gmail.com>
var koLocale = {
    abbr: 'ko',
    months: '1월_2월_3월_4월_5월_6월_7월_8월_9월_10월_11월_12월'.split('_'),
    monthsShort: '1월_2월_3월_4월_5월_6월_7월_8월_9월_10월_11월_12월'.split('_'),
    weekdays: '일요일_월요일_화요일_수요일_목요일_금요일_토요일'.split('_'),
    weekdaysShort: '일_월_화_수_목_금_토'.split('_'),
    weekdaysMin: '일_월_화_수_목_금_토'.split('_'),
    longDateFormat: {
        LT: 'A h:mm',
        LTS: 'A h:mm:ss',
        L: 'YYYY.MM.DD',
        LL: 'YYYY년 MMMM D일',
        LLL: 'YYYY년 MMMM D일 A h:mm',
        LLLL: 'YYYY년 MMMM D일 dddd A h:mm',
        l: 'YYYY.MM.DD',
        ll: 'YYYY년 MMMM D일',
        lll: 'YYYY년 MMMM D일 A h:mm',
        llll: 'YYYY년 MMMM D일 dddd A h:mm'
    },
    calendar: {
        sameDay: '오늘 LT',
        nextDay: '내일 LT',
        nextWeek: 'dddd LT',
        lastDay: '어제 LT',
        lastWeek: '지난주 dddd LT',
        sameElse: 'L'
    },
    relativeTime: {
        future: '%s 후',
        past: '%s 전',
        s: '몇 초',
        ss: '%d초',
        m: '1분',
        mm: '%d분',
        h: '한 시간',
        hh: '%d시간',
        d: '하루',
        dd: '%d일',
        M: '한 달',
        MM: '%d달',
        y: '일 년',
        yy: '%d년'
    },
    dayOfMonthOrdinalParse: /\d{1,2}(일|월|주)/,
    ordinal: function (num, period) {
        switch (period) {
            case 'd':
            case 'D':
            case 'DDD':
                return num + '일';
            case 'M':
                return num + '월';
            case 'w':
            case 'W':
                return num + '주';
            default:
                return num.toString(10);
        }
    },
    meridiemParse: /오전|오후/,
    isPM: function (token) {
        return token === '오후';
    },
    meridiem: function (hour, minute, isUpper) {
        return hour < 12 ? '오전' : '오후';
    }
};
//# sourceMappingURL=ko.js.map

/***/ }),

/***/ "./node_modules/ngx-bootstrap/chronos/i18n/mn.js":
/*!*******************************************************!*\
  !*** ./node_modules/ngx-bootstrap/chronos/i18n/mn.js ***!
  \*******************************************************/
/*! exports provided: mnLocale */
/***/ (function(module, __webpack_exports__, __webpack_require__) {

"use strict";
__webpack_require__.r(__webpack_exports__);
/* harmony export (binding) */ __webpack_require__.d(__webpack_exports__, "mnLocale", function() { return mnLocale; });
// tslint:disable:comment-format binary-expression-operand-order max-line-length
// tslint:disable:no-bitwise prefer-template cyclomatic-complexity
// tslint:disable:no-shadowed-variable switch-default prefer-const
// tslint:disable:one-variable-per-declaration newline-before-return
// tslint:disable:object-literal-shorthand
//! moment.js locale configuration
//! locale : Mongolian [mn]
//! author : Javkhlantugs Nyamdorj : https://github.com/javkhaanj7
function translate(num, withoutSuffix, key, isFuture) {
    switch (key) {
        case 's':
            return withoutSuffix ? 'хэдхэн секунд' : 'хэдхэн секундын';
        case 'ss':
            return num + (withoutSuffix ? ' секунд' : ' секундын');
        case 'm':
        case 'mm':
            return num + (withoutSuffix ? ' минут' : ' минутын');
        case 'h':
        case 'hh':
            return num + (withoutSuffix ? ' цаг' : ' цагийн');
        case 'd':
        case 'dd':
            return num + (withoutSuffix ? ' өдөр' : ' өдрийн');
        case 'M':
        case 'MM':
            return num + (withoutSuffix ? ' сар' : ' сарын');
        case 'y':
        case 'yy':
            return num + (withoutSuffix ? ' жил' : ' жилийн');
        default:
            return num.toString(10);
    }
}
var mnLocale = {
    abbr: 'mn',
    months: 'Нэгдүгээр сар_Хоёрдугаар сар_Гуравдугаар сар_Дөрөвдүгээр сар_Тавдугаар сар_Зургадугаар сар_Долдугаар сар_Наймдугаар сар_Есдүгээр сар_Аравдугаар сар_Арван нэгдүгээр сар_Арван хоёрдугаар сар'.split('_'),
    monthsShort: '1 сар_2 сар_3 сар_4 сар_5 сар_6 сар_7 сар_8 сар_9 сар_10 сар_11 сар_12 сар'.split('_'),
    monthsParseExact: true,
    weekdays: 'Ням_Даваа_Мягмар_Лхагва_Пүрэв_Баасан_Бямба'.split('_'),
    weekdaysShort: 'Ням_Дав_Мяг_Лха_Пүр_Баа_Бям'.split('_'),
    weekdaysMin: 'Ня_Да_Мя_Лх_Пү_Ба_Бя'.split('_'),
    weekdaysParseExact: true,
    longDateFormat: {
        LT: 'HH:mm',
        LTS: 'HH:mm:ss',
        L: 'YYYY-MM-DD',
        LL: 'YYYY оны MMMMын D',
        LLL: 'YYYY оны MMMMын D HH:mm',
        LLLL: 'dddd, YYYY оны MMMMын D HH:mm'
    },
    meridiemParse: /ҮӨ|ҮХ/i,
    isPM: function (input) {
        return input === 'ҮХ';
    },
    meridiem: function (hour, minute, isLower) {
        if (hour < 12) {
            return 'ҮӨ';
        }
        else {
            return 'ҮХ';
        }
    },
    calendar: {
        sameDay: '[Өнөөдөр] LT',
        nextDay: '[Маргааш] LT',
        nextWeek: '[Ирэх] dddd LT',
        lastDay: '[Өчигдөр] LT',
        lastWeek: '[Өнгөрсөн] dddd LT',
        sameElse: 'L'
    },
    relativeTime: {
        future: '%s дараа',
        past: '%s өмнө',
        s: translate,
        ss: translate,
        m: translate,
        mm: translate,
        h: translate,
        hh: translate,
        d: translate,
        dd: translate,
        M: translate,
        MM: translate,
        y: translate,
        yy: translate
    },
    dayOfMonthOrdinalParse: /\d{1,2} өдөр/,
    ordinal: function (num, period) {
        switch (period) {
            case 'd':
            case 'D':
            case 'DDD':
                return num + ' өдөр';
            default:
                return num.toString(10);
        }
    }
};
//# sourceMappingURL=mn.js.map

/***/ }),

/***/ "./node_modules/ngx-bootstrap/chronos/i18n/nl-be.js":
/*!**********************************************************!*\
  !*** ./node_modules/ngx-bootstrap/chronos/i18n/nl-be.js ***!
  \**********************************************************/
/*! exports provided: nlBeLocale */
/***/ (function(module, __webpack_exports__, __webpack_require__) {

"use strict";
__webpack_require__.r(__webpack_exports__);
/* harmony export (binding) */ __webpack_require__.d(__webpack_exports__, "nlBeLocale", function() { return nlBeLocale; });
/* harmony import */ var _utils_date_getters__WEBPACK_IMPORTED_MODULE_0__ = __webpack_require__(/*! ../utils/date-getters */ "./node_modules/ngx-bootstrap/chronos/utils/date-getters.js");
// tslint:disable:comment-format binary-expression-operand-order max-line-length
// tslint:disable:no-bitwise prefer-template cyclomatic-complexity
// tslint:disable:no-shadowed-variable switch-default prefer-const
// tslint:disable:one-variable-per-declaration newline-before-return

//! moment.js locale configuration
//! locale : Dutch (Belgium) [nl-be]
//! author : Joris Röling : https://github.com/jorisroling
//! author : Jacob Middag : https://github.com/middagj
var monthsShortWithDots = 'jan._feb._mrt._apr._mei_jun._jul._aug._sep._okt._nov._dec.'.split('_');
var monthsShortWithoutDots = 'jan_feb_mrt_apr_mei_jun_jul_aug_sep_okt_nov_dec'.split('_');
var monthsParse = [/^jan/i, /^feb/i, /^maart|mrt.?$/i, /^apr/i, /^mei$/i, /^jun[i.]?$/i, /^jul[i.]?$/i, /^aug/i, /^sep/i, /^okt/i, /^nov/i, /^dec/i];
var monthsRegex = /^(januari|februari|maart|april|mei|april|ju[nl]i|augustus|september|oktober|november|december|jan\.?|feb\.?|mrt\.?|apr\.?|ju[nl]\.?|aug\.?|sep\.?|okt\.?|nov\.?|dec\.?)/i;
var nlBeLocale = {
    abbr: 'nl-be',
    months: 'januari_februari_maart_april_mei_juni_juli_augustus_september_oktober_november_december'.split('_'),
    monthsShort: function (date, format, isUTC) {
        if (!date) {
            return monthsShortWithDots;
        }
        else if (/-MMM-/.test(format)) {
            return monthsShortWithoutDots[Object(_utils_date_getters__WEBPACK_IMPORTED_MODULE_0__["getMonth"])(date, isUTC)];
        }
        else {
            return monthsShortWithDots[Object(_utils_date_getters__WEBPACK_IMPORTED_MODULE_0__["getMonth"])(date, isUTC)];
        }
    },
    monthsRegex: monthsRegex,
    monthsShortRegex: monthsRegex,
    monthsStrictRegex: /^(januari|februari|maart|mei|ju[nl]i|april|augustus|september|oktober|november|december)/i,
    monthsShortStrictRegex: /^(jan\.?|feb\.?|mrt\.?|apr\.?|mei|ju[nl]\.?|aug\.?|sep\.?|okt\.?|nov\.?|dec\.?)/i,
    monthsParse: monthsParse,
    longMonthsParse: monthsParse,
    shortMonthsParse: monthsParse,
    weekdays: 'zondag_maandag_dinsdag_woensdag_donderdag_vrijdag_zaterdag'.split('_'),
    weekdaysShort: 'zo._ma._di._wo._do._vr._za.'.split('_'),
    weekdaysMin: 'zo_ma_di_wo_do_vr_za'.split('_'),
    weekdaysParseExact: true,
    longDateFormat: {
        LT: 'HH:mm',
        LTS: 'HH:mm:ss',
        L: 'DD/MM/YYYY',
        LL: 'D MMMM YYYY',
        LLL: 'D MMMM YYYY HH:mm',
        LLLL: 'dddd D MMMM YYYY HH:mm'
    },
    calendar: {
        sameDay: '[vandaag om] LT',
        nextDay: '[morgen om] LT',
        nextWeek: 'dddd [om] LT',
        lastDay: '[gisteren om] LT',
        lastWeek: '[afgelopen] dddd [om] LT',
        sameElse: 'L'
    },
    relativeTime: {
        future: 'over %s',
        past: '%s geleden',
        s: 'een paar seconden',
        ss: '%d seconden',
        m: 'één minuut',
        mm: '%d minuten',
        h: 'één uur',
        hh: '%d uur',
        d: 'één dag',
        dd: '%d dagen',
        M: 'één maand',
        MM: '%d maanden',
        y: 'één jaar',
        yy: '%d jaar'
    },
    dayOfMonthOrdinalParse: /\d{1,2}(ste|de)/,
    ordinal: function (_num) {
        var num = Number(_num);
        return num + ((num === 1 || num === 8 || num >= 20) ? 'ste' : 'de');
    },
    week: {
        dow: 1,
        // Monday is the first day of the week.
        doy: 4 // The week that contains Jan 4th is the first week of the year.
    }
};
//# sourceMappingURL=nl-be.js.map

/***/ }),

/***/ "./node_modules/ngx-bootstrap/chronos/i18n/nl.js":
/*!*******************************************************!*\
  !*** ./node_modules/ngx-bootstrap/chronos/i18n/nl.js ***!
  \*******************************************************/
/*! exports provided: nlLocale */
/***/ (function(module, __webpack_exports__, __webpack_require__) {

"use strict";
__webpack_require__.r(__webpack_exports__);
/* harmony export (binding) */ __webpack_require__.d(__webpack_exports__, "nlLocale", function() { return nlLocale; });
/* harmony import */ var _utils_date_getters__WEBPACK_IMPORTED_MODULE_0__ = __webpack_require__(/*! ../utils/date-getters */ "./node_modules/ngx-bootstrap/chronos/utils/date-getters.js");
// tslint:disable:comment-format binary-expression-operand-order max-line-length
// tslint:disable:no-bitwise prefer-template cyclomatic-complexity
// tslint:disable:no-shadowed-variable switch-default prefer-const
// tslint:disable:one-variable-per-declaration newline-before-return

//! moment.js locale configuration
//! locale : Dutch [nl]
//! author : Joris Röling : https://github.com/jorisroling
//! author : Jacob Middag : https://github.com/middagj
var monthsShortWithDots = 'jan._feb._mrt._apr._mei_jun._jul._aug._sep._okt._nov._dec.'.split('_'), monthsShortWithoutDots = 'jan_feb_mrt_apr_mei_jun_jul_aug_sep_okt_nov_dec'.split('_');
var monthsParse = [/^jan/i, /^feb/i, /^maart|mrt.?$/i, /^apr/i, /^mei$/i, /^jun[i.]?$/i, /^jul[i.]?$/i, /^aug/i, /^sep/i, /^okt/i, /^nov/i, /^dec/i];
var monthsRegex = /^(januari|februari|maart|april|mei|april|ju[nl]i|augustus|september|oktober|november|december|jan\.?|feb\.?|mrt\.?|apr\.?|ju[nl]\.?|aug\.?|sep\.?|okt\.?|nov\.?|dec\.?)/i;
var nlLocale = {
    abbr: 'nl',
    months: 'januari_februari_maart_april_mei_juni_juli_augustus_september_oktober_november_december'.split('_'),
    monthsShort: function (date, format, isUTC) {
        if (!date) {
            return monthsShortWithDots;
        }
        else if (/-MMM-/.test(format)) {
            return monthsShortWithoutDots[Object(_utils_date_getters__WEBPACK_IMPORTED_MODULE_0__["getMonth"])(date, isUTC)];
        }
        else {
            return monthsShortWithDots[Object(_utils_date_getters__WEBPACK_IMPORTED_MODULE_0__["getMonth"])(date, isUTC)];
        }
    },
    monthsRegex: monthsRegex,
    monthsShortRegex: monthsRegex,
    monthsStrictRegex: /^(januari|februari|maart|mei|ju[nl]i|april|augustus|september|oktober|november|december)/i,
    monthsShortStrictRegex: /^(jan\.?|feb\.?|mrt\.?|apr\.?|mei|ju[nl]\.?|aug\.?|sep\.?|okt\.?|nov\.?|dec\.?)/i,
    monthsParse: monthsParse,
    longMonthsParse: monthsParse,
    shortMonthsParse: monthsParse,
    weekdays: 'zondag_maandag_dinsdag_woensdag_donderdag_vrijdag_zaterdag'.split('_'),
    weekdaysShort: 'zo._ma._di._wo._do._vr._za.'.split('_'),
    weekdaysMin: 'zo_ma_di_wo_do_vr_za'.split('_'),
    weekdaysParseExact: true,
    longDateFormat: {
        LT: 'HH:mm',
        LTS: 'HH:mm:ss',
        L: 'DD-MM-YYYY',
        LL: 'D MMMM YYYY',
        LLL: 'D MMMM YYYY HH:mm',
        LLLL: 'dddd D MMMM YYYY HH:mm'
    },
    calendar: {
        sameDay: '[vandaag om] LT',
        nextDay: '[morgen om] LT',
        nextWeek: 'dddd [om] LT',
        lastDay: '[gisteren om] LT',
        lastWeek: '[afgelopen] dddd [om] LT',
        sameElse: 'L'
    },
    relativeTime: {
        future: 'over %s',
        past: '%s geleden',
        s: 'een paar seconden',
        ss: '%d seconden',
        m: 'één minuut',
        mm: '%d minuten',
        h: 'één uur',
        hh: '%d uur',
        d: 'één dag',
        dd: '%d dagen',
        M: 'één maand',
        MM: '%d maanden',
        y: 'één jaar',
        yy: '%d jaar'
    },
    dayOfMonthOrdinalParse: /\d{1,2}(ste|de)/,
    ordinal: function (_num) {
        var num = Number(_num);
        return num + ((num === 1 || num === 8 || num >= 20) ? 'ste' : 'de');
    },
    week: {
        dow: 1,
        // Monday is the first day of the week.
        doy: 4 // The week that contains Jan 4th is the first week of the year.
    }
};
//# sourceMappingURL=nl.js.map

/***/ }),

/***/ "./node_modules/ngx-bootstrap/chronos/i18n/pl.js":
/*!*******************************************************!*\
  !*** ./node_modules/ngx-bootstrap/chronos/i18n/pl.js ***!
  \*******************************************************/
/*! exports provided: plLocale */
/***/ (function(module, __webpack_exports__, __webpack_require__) {

"use strict";
__webpack_require__.r(__webpack_exports__);
/* harmony export (binding) */ __webpack_require__.d(__webpack_exports__, "plLocale", function() { return plLocale; });
/* harmony import */ var _utils_date_getters__WEBPACK_IMPORTED_MODULE_0__ = __webpack_require__(/*! ../utils/date-getters */ "./node_modules/ngx-bootstrap/chronos/utils/date-getters.js");
/* harmony import */ var _units_day_of_week__WEBPACK_IMPORTED_MODULE_1__ = __webpack_require__(/*! ../units/day-of-week */ "./node_modules/ngx-bootstrap/chronos/units/day-of-week.js");
// tslint:disable:comment-format binary-expression-operand-order max-line-length
// tslint:disable:no-bitwise prefer-template cyclomatic-complexity
// tslint:disable:no-shadowed-variable switch-default prefer-const
// tslint:disable:one-variable-per-declaration newline-before-return


//! moment.js locale configuration
//! locale : Polish [pl]
//! author : Rafal Hirsz : https://github.com/evoL
var monthsNominative = 'styczeń_luty_marzec_kwiecień_maj_czerwiec_lipiec_sierpień_wrzesień_październik_listopad_grudzień'.split('_');
var monthsSubjective = 'stycznia_lutego_marca_kwietnia_maja_czerwca_lipca_sierpnia_września_października_listopada_grudnia'.split('_');
function plural(num) {
    return (num % 10 < 5) && (num % 10 > 1) && ((~~(num / 10) % 10) !== 1);
}
function translate(num, withoutSuffix, key) {
    var result = num + ' ';
    switch (key) {
        case 'ss':
            return result + (plural(num) ? 'sekundy' : 'sekund');
        case 'm':
            return withoutSuffix ? 'minuta' : 'minutę';
        case 'mm':
            return result + (plural(num) ? 'minuty' : 'minut');
        case 'h':
            return withoutSuffix ? 'godzina' : 'godzinę';
        case 'hh':
            return result + (plural(num) ? 'godziny' : 'godzin');
        case 'MM':
            return result + (plural(num) ? 'miesiące' : 'miesięcy');
        case 'yy':
            return result + (plural(num) ? 'lata' : 'lat');
    }
}
var plLocale = {
    abbr: 'pl',
    months: function (date, format, isUTC) {
        if (!date) {
            return monthsNominative;
        }
        else if (format === '') {
            // Hack: if format empty we know this is used to generate
            // RegExp by moment. Give then back both valid forms of months
            // in RegExp ready format.
            return '(' + monthsSubjective[Object(_utils_date_getters__WEBPACK_IMPORTED_MODULE_0__["getMonth"])(date, isUTC)] + '|' + monthsNominative[Object(_utils_date_getters__WEBPACK_IMPORTED_MODULE_0__["getMonth"])(date, isUTC)] + ')';
        }
        else if (/D MMMM/.test(format)) {
            return monthsSubjective[Object(_utils_date_getters__WEBPACK_IMPORTED_MODULE_0__["getMonth"])(date, isUTC)];
        }
        else {
            return monthsNominative[Object(_utils_date_getters__WEBPACK_IMPORTED_MODULE_0__["getMonth"])(date, isUTC)];
        }
    },
    monthsShort: 'sty_lut_mar_kwi_maj_cze_lip_sie_wrz_paź_lis_gru'.split('_'),
    weekdays: 'niedziela_poniedziałek_wtorek_środa_czwartek_piątek_sobota'.split('_'),
    weekdaysShort: 'ndz_pon_wt_śr_czw_pt_sob'.split('_'),
    weekdaysMin: 'Nd_Pn_Wt_Śr_Cz_Pt_So'.split('_'),
    longDateFormat: {
        LT: 'HH:mm',
        LTS: 'HH:mm:ss',
        L: 'DD.MM.YYYY',
        LL: 'D MMMM YYYY',
        LLL: 'D MMMM YYYY HH:mm',
        LLLL: 'dddd, D MMMM YYYY HH:mm'
    },
    calendar: {
        sameDay: '[Dziś o] LT',
        nextDay: '[Jutro o] LT',
        nextWeek: function (date) {
            switch (Object(_units_day_of_week__WEBPACK_IMPORTED_MODULE_1__["getDayOfWeek"])(date)) {
                case 0:
                    return '[W niedzielę o] LT';
                case 2:
                    return '[We wtorek o] LT';
                case 3:
                    return '[W środę o] LT';
                case 6:
                    return '[W sobotę o] LT';
                default:
                    return '[W] dddd [o] LT';
            }
        },
        lastDay: '[Wczoraj o] LT',
        lastWeek: function (date) {
            switch (Object(_units_day_of_week__WEBPACK_IMPORTED_MODULE_1__["getDayOfWeek"])(date)) {
                case 0:
                    return '[W zeszłą niedzielę o] LT';
                case 3:
                    return '[W zeszłą środę o] LT';
                case 6:
                    return '[W zeszłą sobotę o] LT';
                default:
                    return '[W zeszły] dddd [o] LT';
            }
        },
        sameElse: 'L'
    },
    relativeTime: {
        future: 'za %s',
        past: '%s temu',
        s: 'kilka sekund',
        ss: translate,
        m: translate,
        mm: translate,
        h: translate,
        hh: translate,
        d: '1 dzień',
        dd: '%d dni',
        M: 'miesiąc',
        MM: translate,
        y: 'rok',
        yy: translate
    },
    dayOfMonthOrdinalParse: /\d{1,2}\./,
    ordinal: '%d.',
    week: {
        dow: 1,
        // Monday is the first day of the week.
        doy: 4 // The week that contains Jan 4th is the first week of the year.
    }
};
//# sourceMappingURL=pl.js.map

/***/ }),

/***/ "./node_modules/ngx-bootstrap/chronos/i18n/pt-br.js":
/*!**********************************************************!*\
  !*** ./node_modules/ngx-bootstrap/chronos/i18n/pt-br.js ***!
  \**********************************************************/
/*! exports provided: ptBrLocale */
/***/ (function(module, __webpack_exports__, __webpack_require__) {

"use strict";
__webpack_require__.r(__webpack_exports__);
/* harmony export (binding) */ __webpack_require__.d(__webpack_exports__, "ptBrLocale", function() { return ptBrLocale; });
/* harmony import */ var _units_day_of_week__WEBPACK_IMPORTED_MODULE_0__ = __webpack_require__(/*! ../units/day-of-week */ "./node_modules/ngx-bootstrap/chronos/units/day-of-week.js");
// tslint:disable:comment-format binary-expression-operand-order max-line-length
// tslint:disable:no-bitwise prefer-template cyclomatic-complexity
// tslint:disable:no-shadowed-variable switch-default prefer-const
// tslint:disable:one-variable-per-declaration newline-before-return

//! moment.js locale configuration
//! locale : Portuguese (Brazil) [pt-br]
//! author : Caio Ribeiro Pereira : https://github.com/caio-ribeiro-pereira
var ptBrLocale = {
    abbr: 'pt-br',
    months: 'janeiro_fevereiro_março_abril_maio_junho_julho_agosto_setembro_outubro_novembro_dezembro'.split('_'),
    monthsShort: 'jan_fev_mar_abr_mai_jun_jul_ago_set_out_nov_dez'.split('_'),
    weekdays: 'Domingo_Segunda-feira_Terça-feira_Quarta-feira_Quinta-feira_Sexta-feira_Sábado'.split('_'),
    weekdaysShort: 'Dom_Seg_Ter_Qua_Qui_Sex_Sáb'.split('_'),
    weekdaysMin: 'Do_2ª_3ª_4ª_5ª_6ª_Sá'.split('_'),
    weekdaysParseExact: true,
    longDateFormat: {
        LT: 'HH:mm',
        LTS: 'HH:mm:ss',
        L: 'DD/MM/YYYY',
        LL: 'D [de] MMMM [de] YYYY',
        LLL: 'D [de] MMMM [de] YYYY [às] HH:mm',
        LLLL: 'dddd, D [de] MMMM [de] YYYY [às] HH:mm'
    },
    calendar: {
        sameDay: '[Hoje às] LT',
        nextDay: '[Amanhã às] LT',
        nextWeek: 'dddd [às] LT',
        lastDay: '[Ontem às] LT',
        lastWeek: function (date) {
            return (Object(_units_day_of_week__WEBPACK_IMPORTED_MODULE_0__["getDayOfWeek"])(date) === 0 || Object(_units_day_of_week__WEBPACK_IMPORTED_MODULE_0__["getDayOfWeek"])(date) === 6) ?
                '[Último] dddd [às] LT' : // Saturday + Sunday
                '[Última] dddd [às] LT'; // Monday - Friday
        },
        sameElse: 'L'
    },
    relativeTime: {
        future: 'em %s',
        past: '%s atrás',
        s: 'poucos segundos',
        ss: '%d segundos',
        m: 'um minuto',
        mm: '%d minutos',
        h: 'uma hora',
        hh: '%d horas',
        d: 'um dia',
        dd: '%d dias',
        M: 'um mês',
        MM: '%d meses',
        y: 'um ano',
        yy: '%d anos'
    },
    dayOfMonthOrdinalParse: /\d{1,2}º/,
    ordinal: '%dº'
};
//# sourceMappingURL=pt-br.js.map

/***/ }),

/***/ "./node_modules/ngx-bootstrap/chronos/i18n/ro.js":
/*!*******************************************************!*\
  !*** ./node_modules/ngx-bootstrap/chronos/i18n/ro.js ***!
  \*******************************************************/
/*! exports provided: roLocale */
/***/ (function(module, __webpack_exports__, __webpack_require__) {

"use strict";
__webpack_require__.r(__webpack_exports__);
/* harmony export (binding) */ __webpack_require__.d(__webpack_exports__, "roLocale", function() { return roLocale; });
// ! moment.js locale configuration
// ! locale : Romanian [ro]
//! author : Vlad Gurdiga : https://github.com/gurdiga
//! author : Valentin Agachi : https://github.com/avaly
// ! author : Earle white: https://github.com/5earle
function relativeTimeWithPlural(num, withoutSuffix, key) {
    var format = {
        ss: 'secunde',
        mm: 'minute',
        hh: 'ore',
        dd: 'zile',
        MM: 'luni',
        yy: 'ani'
    };
    var separator = ' ';
    if (num % 100 >= 20 || (num >= 100 && num % 100 === 0)) {
        separator = ' de ';
    }
    return num + separator + format[key];
}
var roLocale = {
    abbr: 'ro',
    months: 'ianuarie_februarie_martie_aprilie_mai_iunie_iulie_august_septembrie_octombrie_noiembrie_decembrie'.split('_'),
    monthsShort: 'ian._febr._mart._apr._mai_iun._iul._aug._sept._oct._nov._dec.'.split('_'),
    monthsParseExact: true,
    weekdays: 'duminică_luni_marți_miercuri_joi_vineri_sâmbătă'.split('_'),
    weekdaysShort: 'Dum_Lun_Mar_Mie_Joi_Vin_Sâm'.split('_'),
    weekdaysMin: 'Du_Lu_Ma_Mi_Jo_Vi_Sâ'.split('_'),
    longDateFormat: {
        LT: 'H:mm',
        LTS: 'H:mm:ss',
        L: 'DD.MM.YYYY',
        LL: 'D MMMM YYYY',
        LLL: 'D MMMM YYYY H:mm',
        LLLL: 'dddd, D MMMM YYYY H:mm'
    },
    calendar: {
        sameDay: '[azi la] LT',
        nextDay: '[mâine la] LT',
        nextWeek: 'dddd [la] LT',
        lastDay: '[ieri la] LT',
        lastWeek: '[fosta] dddd [la] LT',
        sameElse: 'L'
    },
    relativeTime: {
        future: 'peste %s',
        past: '%s în urmă',
        s: 'câteva secunde',
        ss: relativeTimeWithPlural,
        m: 'un minut',
        mm: relativeTimeWithPlural,
        h: 'o oră',
        hh: relativeTimeWithPlural,
        d: 'o zi',
        dd: relativeTimeWithPlural,
        M: 'o lună',
        MM: relativeTimeWithPlural,
        y: 'un an',
        yy: relativeTimeWithPlural
    },
    week: {
        dow: 1,
        // Monday is the first day of the week.
        doy: 7 // The week that contains Jan 1st is the first week of the year.
    }
};
//# sourceMappingURL=ro.js.map

/***/ }),

/***/ "./node_modules/ngx-bootstrap/chronos/i18n/ru.js":
/*!*******************************************************!*\
  !*** ./node_modules/ngx-bootstrap/chronos/i18n/ru.js ***!
  \*******************************************************/
/*! exports provided: ruLocale */
/***/ (function(module, __webpack_exports__, __webpack_require__) {

"use strict";
__webpack_require__.r(__webpack_exports__);
/* harmony export (binding) */ __webpack_require__.d(__webpack_exports__, "ruLocale", function() { return ruLocale; });
/* harmony import */ var _units_week__WEBPACK_IMPORTED_MODULE_0__ = __webpack_require__(/*! ../units/week */ "./node_modules/ngx-bootstrap/chronos/units/week.js");
/* harmony import */ var _units_day_of_week__WEBPACK_IMPORTED_MODULE_1__ = __webpack_require__(/*! ../units/day-of-week */ "./node_modules/ngx-bootstrap/chronos/units/day-of-week.js");
// tslint:disable:comment-format binary-expression-operand-order max-line-length
// tslint:disable:no-bitwise prefer-template cyclomatic-complexity
// tslint:disable:no-shadowed-variable switch-default prefer-const
// tslint:disable:one-variable-per-declaration newline-before-return


//! moment.js locale configuration
//! locale : Russian [ru]
//! author : Viktorminator : https://github.com/Viktorminator
//! Author : Menelion Elensúle : https://github.com/Oire
//! author : Коренберг Марк : https://github.com/socketpair
function plural(word, num) {
    var forms = word.split('_');
    return num % 10 === 1 && num % 100 !== 11 ? forms[0] : (num % 10 >= 2 && num % 10 <= 4 && (num % 100 < 10 || num % 100 >= 20) ? forms[1] : forms[2]);
}
function relativeTimeWithPlural(num, withoutSuffix, key) {
    var format = {
        ss: withoutSuffix ? 'секунда_секунды_секунд' : 'секунду_секунды_секунд',
        mm: withoutSuffix ? 'минута_минуты_минут' : 'минуту_минуты_минут',
        hh: 'час_часа_часов',
        dd: 'день_дня_дней',
        MM: 'месяц_месяца_месяцев',
        yy: 'год_года_лет'
    };
    if (key === 'm') {
        return withoutSuffix ? 'минута' : 'минуту';
    }
    return num + ' ' + plural(format[key], +num);
}
var monthsParse = [/^янв/i, /^фев/i, /^мар/i, /^апр/i, /^ма[йя]/i, /^июн/i, /^июл/i, /^авг/i, /^сен/i, /^окт/i, /^ноя/i, /^дек/i];
// http://new.gramota.ru/spravka/rules/139-prop : § 103
// Сокращения месяцев: http://new.gramota.ru/spravka/buro/search-answer?s=242637
// CLDR data:          http://www.unicode.org/cldr/charts/28/summary/ru.html#1753
var ruLocale = {
    abbr: 'ru',
    months: {
        format: 'января_февраля_марта_апреля_мая_июня_июля_августа_сентября_октября_ноября_декабря'.split('_'),
        standalone: 'январь_февраль_март_апрель_май_июнь_июль_август_сентябрь_октябрь_ноябрь_декабрь'.split('_')
    },
    monthsShort: {
        // по CLDR именно "июл." и "июн.", но какой смысл менять букву на точку ?
        format: 'янв._февр._мар._апр._мая_июня_июля_авг._сент._окт._нояб._дек.'.split('_'),
        standalone: 'янв._февр._март_апр._май_июнь_июль_авг._сент._окт._нояб._дек.'.split('_')
    },
    weekdays: {
        standalone: 'воскресенье_понедельник_вторник_среда_четверг_пятница_суббота'.split('_'),
        format: 'воскресенье_понедельник_вторник_среду_четверг_пятницу_субботу'.split('_'),
        isFormat: /\[ ?[Вв] ?(?:прошлую|следующую|эту)? ?\] ?dddd/
    },
    weekdaysShort: 'вс_пн_вт_ср_чт_пт_сб'.split('_'),
    weekdaysMin: 'вс_пн_вт_ср_чт_пт_сб'.split('_'),
    monthsParse: monthsParse,
    longMonthsParse: monthsParse,
    shortMonthsParse: monthsParse,
    // полные названия с падежами, по три буквы, для некоторых, по 4 буквы, сокращения с точкой и без точки
    monthsRegex: /^(январ[ья]|янв\.?|феврал[ья]|февр?\.?|марта?|мар\.?|апрел[ья]|апр\.?|ма[йя]|июн[ья]|июн\.?|июл[ья]|июл\.?|августа?|авг\.?|сентябр[ья]|сент?\.?|октябр[ья]|окт\.?|ноябр[ья]|нояб?\.?|декабр[ья]|дек\.?)/i,
    // копия предыдущего
    monthsShortRegex: /^(январ[ья]|янв\.?|феврал[ья]|февр?\.?|марта?|мар\.?|апрел[ья]|апр\.?|ма[йя]|июн[ья]|июн\.?|июл[ья]|июл\.?|августа?|авг\.?|сентябр[ья]|сент?\.?|октябр[ья]|окт\.?|ноябр[ья]|нояб?\.?|декабр[ья]|дек\.?)/i,
    // полные названия с падежами
    monthsStrictRegex: /^(январ[яь]|феврал[яь]|марта?|апрел[яь]|ма[яй]|июн[яь]|июл[яь]|августа?|сентябр[яь]|октябр[яь]|ноябр[яь]|декабр[яь])/i,
    // Выражение, которое соотвествует только сокращённым формам
    monthsShortStrictRegex: /^(янв\.|февр?\.|мар[т.]|апр\.|ма[яй]|июн[ья.]|июл[ья.]|авг\.|сент?\.|окт\.|нояб?\.|дек\.)/i,
    longDateFormat: {
        LT: 'H:mm',
        LTS: 'H:mm:ss',
        L: 'DD.MM.YYYY',
        LL: 'D MMMM YYYY г.',
        LLL: 'D MMMM YYYY г., H:mm',
        LLLL: 'dddd, D MMMM YYYY г., H:mm'
    },
    calendar: {
        sameDay: '[Сегодня в] LT',
        nextDay: '[Завтра в] LT',
        lastDay: '[Вчера в] LT',
        nextWeek: function (date, now) {
            if (Object(_units_week__WEBPACK_IMPORTED_MODULE_0__["getWeek"])(now) !== Object(_units_week__WEBPACK_IMPORTED_MODULE_0__["getWeek"])(date)) {
                switch (Object(_units_day_of_week__WEBPACK_IMPORTED_MODULE_1__["getDayOfWeek"])(date)) {
                    case 0:
                        return '[В следующее] dddd [в] LT';
                    case 1:
                    case 2:
                    case 4:
                        return '[В следующий] dddd [в] LT';
                    case 3:
                    case 5:
                    case 6:
                        return '[В следующую] dddd [в] LT';
                }
            }
            else {
                if (Object(_units_day_of_week__WEBPACK_IMPORTED_MODULE_1__["getDayOfWeek"])(date) === 2) {
                    return '[Во] dddd [в] LT';
                }
                else {
                    return '[В] dddd [в] LT';
                }
            }
        },
        lastWeek: function (date, now) {
            if (Object(_units_week__WEBPACK_IMPORTED_MODULE_0__["getWeek"])(now) !== Object(_units_week__WEBPACK_IMPORTED_MODULE_0__["getWeek"])(date)) {
                switch (Object(_units_day_of_week__WEBPACK_IMPORTED_MODULE_1__["getDayOfWeek"])(date)) {
                    case 0:
                        return '[В прошлое] dddd [в] LT';
                    case 1:
                    case 2:
                    case 4:
                        return '[В прошлый] dddd [в] LT';
                    case 3:
                    case 5:
                    case 6:
                        return '[В прошлую] dddd [в] LT';
                }
            }
            else {
                if (Object(_units_day_of_week__WEBPACK_IMPORTED_MODULE_1__["getDayOfWeek"])(date) === 2) {
                    return '[Во] dddd [в] LT';
                }
                else {
                    return '[В] dddd [в] LT';
                }
            }
        },
        sameElse: 'L'
    },
    relativeTime: {
        future: 'через %s',
        past: '%s назад',
        s: 'несколько секунд',
        ss: relativeTimeWithPlural,
        m: relativeTimeWithPlural,
        mm: relativeTimeWithPlural,
        h: 'час',
        hh: relativeTimeWithPlural,
        d: 'день',
        dd: relativeTimeWithPlural,
        M: 'месяц',
        MM: relativeTimeWithPlural,
        y: 'год',
        yy: relativeTimeWithPlural
    },
    meridiemParse: /ночи|утра|дня|вечера/i,
    isPM: function (input) {
        return /^(дня|вечера)$/.test(input);
    },
    meridiem: function (hour, minute, isLower) {
        if (hour < 4) {
            return 'ночи';
        }
        else if (hour < 12) {
            return 'утра';
        }
        else if (hour < 17) {
            return 'дня';
        }
        else {
            return 'вечера';
        }
    },
    dayOfMonthOrdinalParse: /\d{1,2}-(й|го|я)/,
    ordinal: function (_num, period) {
        var num = Number(_num);
        switch (period) {
            case 'M':
            case 'd':
            case 'DDD':
                return num + '-й';
            case 'D':
                return num + '-го';
            case 'w':
            case 'W':
                return num + '-я';
            default:
                return num.toString(10);
        }
    },
    week: {
        dow: 1,
        // Monday is the first day of the week.
        doy: 4 // The week that contains Jan 4th is the first week of the year.
    }
};
//# sourceMappingURL=ru.js.map

/***/ }),

/***/ "./node_modules/ngx-bootstrap/chronos/i18n/sl.js":
/*!*******************************************************!*\
  !*** ./node_modules/ngx-bootstrap/chronos/i18n/sl.js ***!
  \*******************************************************/
/*! exports provided: slLocale */
/***/ (function(module, __webpack_exports__, __webpack_require__) {

"use strict";
__webpack_require__.r(__webpack_exports__);
/* harmony export (binding) */ __webpack_require__.d(__webpack_exports__, "slLocale", function() { return slLocale; });
/* harmony import */ var _units_day_of_week__WEBPACK_IMPORTED_MODULE_0__ = __webpack_require__(/*! ../units/day-of-week */ "./node_modules/ngx-bootstrap/chronos/units/day-of-week.js");
// tslint:disable:comment-format binary-expression-operand-order max-line-length
// tslint:disable:no-bitwise prefer-template cyclomatic-complexity
// tslint:disable:no-shadowed-variable switch-default prefer-const
// tslint:disable:one-variable-per-declaration newline-before-return
// tslint:disable:object-literal-key-quotes

//! moment.js locale configuration
//! locale : Slovenian [sl]
//! author : mihan : https://github.com/mihan
function processRelativeTime(number, withoutSuffix, key, isFuture) {
    var result = number + ' ';
    switch (key) {
        case 's':
            return withoutSuffix || isFuture ? 'nekaj sekund' : 'nekaj sekundami';
        case 'ss':
            if (number === 1) {
                result += withoutSuffix ? 'sekundo' : 'sekundi';
            }
            else if (number === 2) {
                result += withoutSuffix || isFuture ? 'sekundi' : 'sekundah';
            }
            else if (number < 5) {
                result += withoutSuffix || isFuture ? 'sekunde' : 'sekundah';
            }
            else {
                result += withoutSuffix || isFuture ? 'sekund' : 'sekund';
            }
            return result;
        case 'm':
            return withoutSuffix ? 'ena minuta' : 'eno minuto';
        case 'mm':
            if (number === 1) {
                result += withoutSuffix ? 'minuta' : 'minuto';
            }
            else if (number === 2) {
                result += withoutSuffix || isFuture ? 'minuti' : 'minutama';
            }
            else if (number < 5) {
                result += withoutSuffix || isFuture ? 'minute' : 'minutami';
            }
            else {
                result += withoutSuffix || isFuture ? 'minut' : 'minutami';
            }
            return result;
        case 'h':
            return withoutSuffix ? 'ena ura' : 'eno uro';
        case 'hh':
            if (number === 1) {
                result += withoutSuffix ? 'ura' : 'uro';
            }
            else if (number === 2) {
                result += withoutSuffix || isFuture ? 'uri' : 'urama';
            }
            else if (number < 5) {
                result += withoutSuffix || isFuture ? 'ure' : 'urami';
            }
            else {
                result += withoutSuffix || isFuture ? 'ur' : 'urami';
            }
            return result;
        case 'd':
            return withoutSuffix || isFuture ? 'en dan' : 'enim dnem';
        case 'dd':
            if (number === 1) {
                result += withoutSuffix || isFuture ? 'dan' : 'dnem';
            }
            else if (number === 2) {
                result += withoutSuffix || isFuture ? 'dni' : 'dnevoma';
            }
            else {
                result += withoutSuffix || isFuture ? 'dni' : 'dnevi';
            }
            return result;
        case 'M':
            return withoutSuffix || isFuture ? 'en mesec' : 'enim mesecem';
        case 'MM':
            if (number === 1) {
                result += withoutSuffix || isFuture ? 'mesec' : 'mesecem';
            }
            else if (number === 2) {
                result += withoutSuffix || isFuture ? 'meseca' : 'mesecema';
            }
            else if (number < 5) {
                result += withoutSuffix || isFuture ? 'mesece' : 'meseci';
            }
            else {
                result += withoutSuffix || isFuture ? 'mesecev' : 'meseci';
            }
            return result;
        case 'y':
            return withoutSuffix || isFuture ? 'eno leto' : 'enim letom';
        case 'yy':
            if (number === 1) {
                result += withoutSuffix || isFuture ? 'leto' : 'letom';
            }
            else if (number === 2) {
                result += withoutSuffix || isFuture ? 'leti' : 'letoma';
            }
            else if (number < 5) {
                result += withoutSuffix || isFuture ? 'leta' : 'leti';
            }
            else {
                result += withoutSuffix || isFuture ? 'let' : 'leti';
            }
            return result;
    }
}
var slLocale = {
    abbr: 'sl',
    months: 'januar_februar_marec_april_maj_junij_julij_avgust_september_oktober_november_december'.split('_'),
    monthsShort: 'jan._feb._mar._apr._maj._jun._jul._avg._sep._okt._nov._dec.'.split('_'),
    monthsParseExact: true,
    weekdays: 'nedelja_ponedeljek_torek_sreda_četrtek_petek_sobota'.split('_'),
    weekdaysShort: 'ned._pon._tor._sre._čet._pet._sob.'.split('_'),
    weekdaysMin: 'ne_po_to_sr_če_pe_so'.split('_'),
    weekdaysParseExact: true,
    longDateFormat: {
        LT: 'H:mm',
        LTS: 'H:mm:ss',
        L: 'DD.MM.YYYY',
        LL: 'D. MMMM YYYY',
        LLL: 'D. MMMM YYYY H:mm',
        LLLL: 'dddd, D. MMMM YYYY H:mm'
    },
    calendar: {
        sameDay: '[danes ob] LT',
        nextDay: '[jutri ob] LT',
        nextWeek: function (date) {
            switch (Object(_units_day_of_week__WEBPACK_IMPORTED_MODULE_0__["getDayOfWeek"])(date)) {
                case 0:
                    return '[v] [nedeljo] [ob] LT';
                case 3:
                    return '[v] [sredo] [ob] LT';
                case 6:
                    return '[v] [soboto] [ob] LT';
                case 1:
                case 2:
                case 4:
                case 5:
                    return '[v] dddd [ob] LT';
            }
        },
        lastDay: '[včeraj ob] LT',
        lastWeek: function (date) {
            switch (Object(_units_day_of_week__WEBPACK_IMPORTED_MODULE_0__["getDayOfWeek"])(date)) {
                case 0:
                    return '[prejšnjo] [nedeljo] [ob] LT';
                case 3:
                    return '[prejšnjo] [sredo] [ob] LT';
                case 6:
                    return '[prejšnjo] [soboto] [ob] LT';
                case 1:
                case 2:
                case 4:
                case 5:
                    return '[prejšnji] dddd [ob] LT';
            }
        },
        sameElse: 'L'
    },
    relativeTime: {
        future: 'čez %s',
        past: 'pred %s',
        s: processRelativeTime,
        ss: processRelativeTime,
        m: processRelativeTime,
        mm: processRelativeTime,
        h: processRelativeTime,
        hh: processRelativeTime,
        d: processRelativeTime,
        dd: processRelativeTime,
        M: processRelativeTime,
        MM: processRelativeTime,
        y: processRelativeTime,
        yy: processRelativeTime
    },
    dayOfMonthOrdinalParse: /\d{1,2}\./,
    ordinal: '%d.',
    week: {
        dow: 1,
        // Monday is the first day of the week.
        doy: 7 // The week that contains Jan 1st is the first week of the year.
    }
};
//# sourceMappingURL=sl.js.map

/***/ }),

/***/ "./node_modules/ngx-bootstrap/chronos/i18n/sv.js":
/*!*******************************************************!*\
  !*** ./node_modules/ngx-bootstrap/chronos/i18n/sv.js ***!
  \*******************************************************/
/*! exports provided: svLocale */
/***/ (function(module, __webpack_exports__, __webpack_require__) {

"use strict";
__webpack_require__.r(__webpack_exports__);
/* harmony export (binding) */ __webpack_require__.d(__webpack_exports__, "svLocale", function() { return svLocale; });
// tslint:disable:comment-format binary-expression-operand-order max-line-length
// tslint:disable:no-bitwise prefer-template cyclomatic-complexity
// tslint:disable:no-shadowed-variable switch-default prefer-const
// tslint:disable:one-variable-per-declaration newline-before-return
//! moment.js locale configuration
//! locale : Swedish [sv]
//! author : Jens Alm : https://github.com/ulmus
var svLocale = {
    abbr: 'sv',
    months: 'januari_februari_mars_april_maj_juni_juli_augusti_september_oktober_november_december'.split('_'),
    monthsShort: 'jan_feb_mar_apr_maj_jun_jul_aug_sep_okt_nov_dec'.split('_'),
    weekdays: 'söndag_måndag_tisdag_onsdag_torsdag_fredag_lördag'.split('_'),
    weekdaysShort: 'sön_mån_tis_ons_tor_fre_lör'.split('_'),
    weekdaysMin: 'sö_må_ti_on_to_fr_lö'.split('_'),
    longDateFormat: {
        LT: 'HH:mm',
        LTS: 'HH:mm:ss',
        L: 'YYYY-MM-DD',
        LL: 'D MMMM YYYY',
        LLL: 'D MMMM YYYY [kl.] HH:mm',
        LLLL: 'dddd D MMMM YYYY [kl.] HH:mm',
        lll: 'D MMM YYYY HH:mm',
        llll: 'ddd D MMM YYYY HH:mm'
    },
    calendar: {
        sameDay: '[Idag] LT',
        nextDay: '[Imorgon] LT',
        lastDay: '[Igår] LT',
        nextWeek: '[På] dddd LT',
        lastWeek: '[I] dddd[s] LT',
        sameElse: 'L'
    },
    relativeTime: {
        future: 'om %s',
        past: 'för %s sedan',
        s: 'några sekunder',
        ss: '%d sekunder',
        m: 'en minut',
        mm: '%d minuter',
        h: 'en timme',
        hh: '%d timmar',
        d: 'en dag',
        dd: '%d dagar',
        M: 'en månad',
        MM: '%d månader',
        y: 'ett år',
        yy: '%d år'
    },
    dayOfMonthOrdinalParse: /\d{1,2}(e|a)/,
    ordinal: function (_num) {
        var num = Number(_num);
        var b = num % 10, output = (~~(num % 100 / 10) === 1) ? 'e' :
            (b === 1) ? 'a' :
                (b === 2) ? 'a' :
                    (b === 3) ? 'e' : 'e';
        return num + output;
    },
    week: {
        dow: 1,
        // Monday is the first day of the week.
        doy: 4 // The week that contains Jan 4th is the first week of the year.
    }
};
//# sourceMappingURL=sv.js.map

/***/ }),

/***/ "./node_modules/ngx-bootstrap/chronos/i18n/th.js":
/*!*******************************************************!*\
  !*** ./node_modules/ngx-bootstrap/chronos/i18n/th.js ***!
  \*******************************************************/
/*! exports provided: thLocale */
/***/ (function(module, __webpack_exports__, __webpack_require__) {

"use strict";
__webpack_require__.r(__webpack_exports__);
/* harmony export (binding) */ __webpack_require__.d(__webpack_exports__, "thLocale", function() { return thLocale; });
// tslint:disable:comment-format binary-expression-operand-order max-line-length
// tslint:disable:no-bitwise prefer-template cyclomatic-complexity
// tslint:disable:no-shadowed-variable switch-default prefer-const
// tslint:disable:one-variable-per-declaration newline-before-return
var thLocale = {
    abbr: 'th',
    months: 'มกราคม_กุมภาพันธ์_มีนาคม_เมษายน_พฤษภาคม_มิถุนายน_กรกฎาคม_สิงหาคม_กันยายน_ตุลาคม_พฤศจิกายน_ธันวาคม'.split('_'),
    monthsShort: 'ม.ค._ก.พ._มี.ค._เม.ย._พ.ค._มิ.ย._ก.ค._ส.ค._ก.ย._ต.ค._พ.ย._ธ.ค.'.split('_'),
    monthsParseExact: true,
    weekdays: 'อาทิตย์_จันทร์_อังคาร_พุธ_พฤหัสบดี_ศุกร์_เสาร์'.split('_'),
    weekdaysShort: 'อาทิตย์_จันทร์_อังคาร_พุธ_พฤหัส_ศุกร์_เสาร์'.split('_'),
    // yes, three characters difference
    weekdaysMin: 'อา._จ._อ._พ._พฤ._ศ._ส.'.split('_'),
    weekdaysParseExact: true,
    longDateFormat: {
        LT: 'H:mm',
        LTS: 'H:mm:ss',
        L: 'DD/MM/YYYY',
        LL: 'D MMMM YYYY',
        LLL: 'D MMMM YYYY เวลา H:mm',
        LLLL: 'วันddddที่ D MMMM YYYY เวลา H:mm'
    },
    meridiemParse: /ก่อนเที่ยง|หลังเที่ยง/,
    isPM: function (input) {
        return input === 'หลังเที่ยง';
    },
    meridiem: function (hour, minute, isLower) {
        if (hour < 12) {
            return 'ก่อนเที่ยง';
        }
        else {
            return 'หลังเที่ยง';
        }
    },
    calendar: {
        sameDay: '[วันนี้ เวลา] LT',
        nextDay: '[พรุ่งนี้ เวลา] LT',
        nextWeek: 'dddd[หน้า เวลา] LT',
        lastDay: '[เมื่อวานนี้ เวลา] LT',
        lastWeek: '[วัน]dddd[ที่แล้ว เวลา] LT',
        sameElse: 'L'
    },
    relativeTime: {
        future: 'อีก %s',
        past: '%sที่แล้ว',
        s: 'ไม่กี่วินาที',
        ss: '%d วินาที',
        m: '1 นาที',
        mm: '%d นาที',
        h: '1 ชั่วโมง',
        hh: '%d ชั่วโมง',
        d: '1 วัน',
        dd: '%d วัน',
        M: '1 เดือน',
        MM: '%d เดือน',
        y: '1 ปี',
        yy: '%d ปี'
    }
};
//# sourceMappingURL=th.js.map

/***/ }),

/***/ "./node_modules/ngx-bootstrap/chronos/i18n/tr.js":
/*!*******************************************************!*\
  !*** ./node_modules/ngx-bootstrap/chronos/i18n/tr.js ***!
  \*******************************************************/
/*! exports provided: trLocale */
/***/ (function(module, __webpack_exports__, __webpack_require__) {

"use strict";
__webpack_require__.r(__webpack_exports__);
/* harmony export (binding) */ __webpack_require__.d(__webpack_exports__, "trLocale", function() { return trLocale; });
// tslint:disable:comment-format binary-expression-operand-order max-line-length
// tslint:disable:no-bitwise prefer-template cyclomatic-complexity
// tslint:disable:no-shadowed-variable switch-default prefer-const
// tslint:disable:one-variable-per-declaration newline-before-return
//! moment.js locale configuration
//! locale : Turkish [tr]
//! authors : Erhan Gundogan : https://github.com/erhangundogan,
//!           Burak Yiğit Kaya: https://github.com/BYK
var suffixes = {
    1: '\'inci',
    5: '\'inci',
    8: '\'inci',
    70: '\'inci',
    80: '\'inci',
    2: '\'nci',
    7: '\'nci',
    20: '\'nci',
    50: '\'nci',
    3: '\'üncü',
    4: '\'üncü',
    100: '\'üncü',
    6: '\'ncı',
    9: '\'uncu',
    10: '\'uncu',
    30: '\'uncu',
    60: '\'ıncı',
    90: '\'ıncı'
};
var trLocale = {
    abbr: 'tr',
    months: 'Ocak_Şubat_Mart_Nisan_Mayıs_Haziran_Temmuz_Ağustos_Eylül_Ekim_Kasım_Aralık'.split('_'),
    monthsShort: 'Oca_Şub_Mar_Nis_May_Haz_Tem_Ağu_Eyl_Eki_Kas_Ara'.split('_'),
    weekdays: 'Pazar_Pazartesi_Salı_Çarşamba_Perşembe_Cuma_Cumartesi'.split('_'),
    weekdaysShort: 'Paz_Pts_Sal_Çar_Per_Cum_Cts'.split('_'),
    weekdaysMin: 'Pz_Pt_Sa_Ça_Pe_Cu_Ct'.split('_'),
    longDateFormat: {
        LT: 'HH:mm',
        LTS: 'HH:mm:ss',
        L: 'DD.MM.YYYY',
        LL: 'D MMMM YYYY',
        LLL: 'D MMMM YYYY HH:mm',
        LLLL: 'dddd, D MMMM YYYY HH:mm'
    },
    calendar: {
        sameDay: '[bugün saat] LT',
        nextDay: '[yarın saat] LT',
        nextWeek: '[gelecek] dddd [saat] LT',
        lastDay: '[dün] LT',
        lastWeek: '[geçen] dddd [saat] LT',
        sameElse: 'L'
    },
    relativeTime: {
        future: '%s sonra',
        past: '%s önce',
        s: 'birkaç saniye',
        ss: '%d saniye',
        m: 'bir dakika',
        mm: '%d dakika',
        h: 'bir saat',
        hh: '%d saat',
        d: 'bir gün',
        dd: '%d gün',
        M: 'bir ay',
        MM: '%d ay',
        y: 'bir yıl',
        yy: '%d yıl'
    },
    dayOfMonthOrdinalParse: /\d{1,2}'(inci|nci|üncü|ncı|uncu|ıncı)/,
    ordinal: function (_num) {
        var num = Number(_num);
        if (num === 0) {
            // special case for zero
            return num + '\'ıncı';
        }
        var a = num % 10, b = num % 100 - a, c = num >= 100 ? 100 : null;
        return num + (suffixes[a] || suffixes[b] || suffixes[c]);
    },
    week: {
        dow: 1,
        // Monday is the first day of the week.
        doy: 7 // The week that contains Jan 1st is the first week of the year.
    }
};
//# sourceMappingURL=tr.js.map

/***/ }),

/***/ "./node_modules/ngx-bootstrap/chronos/i18n/zh-cn.js":
/*!**********************************************************!*\
  !*** ./node_modules/ngx-bootstrap/chronos/i18n/zh-cn.js ***!
  \**********************************************************/
/*! exports provided: zhCnLocale */
/***/ (function(module, __webpack_exports__, __webpack_require__) {

"use strict";
__webpack_require__.r(__webpack_exports__);
/* harmony export (binding) */ __webpack_require__.d(__webpack_exports__, "zhCnLocale", function() { return zhCnLocale; });
// tslint:disable:comment-format binary-expression-operand-order max-line-length
// tslint:disable:no-bitwise prefer-template cyclomatic-complexity
// tslint:disable:no-shadowed-variable switch-default prefer-const
// tslint:disable:one-variable-per-declaration newline-before-return
// tslint:disable:no-parameter-reassignment prefer-switch
//! moment.js locale configuration
//! locale : Chinese (China) [zh-cn]
//! author : suupic : https://github.com/suupic
//! author : Zeno Zeng : https://github.com/zenozeng
var zhCnLocale = {
    abbr: 'zh-cn',
    months: '一月_二月_三月_四月_五月_六月_七月_八月_九月_十月_十一月_十二月'.split('_'),
    monthsShort: '1月_2月_3月_4月_5月_6月_7月_8月_9月_10月_11月_12月'.split('_'),
    weekdays: '星期日_星期一_星期二_星期三_星期四_星期五_星期六'.split('_'),
    weekdaysShort: '周日_周一_周二_周三_周四_周五_周六'.split('_'),
    weekdaysMin: '日_一_二_三_四_五_六'.split('_'),
    longDateFormat: {
        LT: 'HH:mm',
        LTS: 'HH:mm:ss',
        L: 'YYYY/MM/DD',
        LL: 'YYYY年M月D日',
        LLL: 'YYYY年M月D日Ah点mm分',
        LLLL: 'YYYY年M月D日ddddAh点mm分',
        l: 'YYYY/M/D',
        ll: 'YYYY年M月D日',
        lll: 'YYYY年M月D日 HH:mm',
        llll: 'YYYY年M月D日dddd HH:mm'
    },
    meridiemParse: /凌晨|早上|上午|中午|下午|晚上/,
    meridiemHour: function (hour, meridiem) {
        if (hour === 12) {
            hour = 0;
        }
        if (meridiem === '凌晨' || meridiem === '早上' ||
            meridiem === '上午') {
            return hour;
        }
        else if (meridiem === '下午' || meridiem === '晚上') {
            return hour + 12;
        }
        else {
            // '中午'
            return hour >= 11 ? hour : hour + 12;
        }
    },
    meridiem: function (hour, minute, isLower) {
        var hm = hour * 100 + minute;
        if (hm < 600) {
            return '凌晨';
        }
        else if (hm < 900) {
            return '早上';
        }
        else if (hm < 1130) {
            return '上午';
        }
        else if (hm < 1230) {
            return '中午';
        }
        else if (hm < 1800) {
            return '下午';
        }
        else {
            return '晚上';
        }
    },
    calendar: {
        sameDay: '[今天]LT',
        nextDay: '[明天]LT',
        nextWeek: '[下]ddddLT',
        lastDay: '[昨天]LT',
        lastWeek: '[上]ddddLT',
        sameElse: 'L'
    },
    dayOfMonthOrdinalParse: /\d{1,2}(日|月|周)/,
    ordinal: function (_num, period) {
        var num = Number(_num);
        switch (period) {
            case 'd':
            case 'D':
            case 'DDD':
                return num + '日';
            case 'M':
                return num + '月';
            case 'w':
            case 'W':
                return num + '周';
            default:
                return num.toString();
        }
    },
    relativeTime: {
        future: '%s内',
        past: '%s前',
        s: '几秒',
        ss: '%d 秒',
        m: '1 分钟',
        mm: '%d 分钟',
        h: '1 小时',
        hh: '%d 小时',
        d: '1 天',
        dd: '%d 天',
        M: '1 个月',
        MM: '%d 个月',
        y: '1 年',
        yy: '%d 年'
    },
    week: {
        // GB/T 7408-1994《数据元和交换格式·信息交换·日期和时间表示法》与ISO 8601:1988等效
        dow: 1,
        // Monday is the first day of the week.
        doy: 4 // The week that contains Jan 4th is the first week of the year.
    }
};
//# sourceMappingURL=zh-cn.js.map

/***/ }),

/***/ "./node_modules/ngx-bootstrap/chronos/index.js":
/*!*****************************************************!*\
  !*** ./node_modules/ngx-bootstrap/chronos/index.js ***!
  \*****************************************************/
/*! exports provided: add, subtract, getMonth, parseDate, formatDate, defineLocale, getSetGlobalLocale, listLocales */
/***/ (function(module, __webpack_exports__, __webpack_require__) {

"use strict";
__webpack_require__.r(__webpack_exports__);
/* harmony import */ var _units_index__WEBPACK_IMPORTED_MODULE_0__ = __webpack_require__(/*! ./units/index */ "./node_modules/ngx-bootstrap/chronos/units/index.js");
/* harmony import */ var _moment_add_subtract__WEBPACK_IMPORTED_MODULE_1__ = __webpack_require__(/*! ./moment/add-subtract */ "./node_modules/ngx-bootstrap/chronos/moment/add-subtract.js");
/* harmony reexport (safe) */ __webpack_require__.d(__webpack_exports__, "add", function() { return _moment_add_subtract__WEBPACK_IMPORTED_MODULE_1__["add"]; });

/* harmony reexport (safe) */ __webpack_require__.d(__webpack_exports__, "subtract", function() { return _moment_add_subtract__WEBPACK_IMPORTED_MODULE_1__["subtract"]; });

/* harmony import */ var _utils_date_getters__WEBPACK_IMPORTED_MODULE_2__ = __webpack_require__(/*! ./utils/date-getters */ "./node_modules/ngx-bootstrap/chronos/utils/date-getters.js");
/* harmony reexport (safe) */ __webpack_require__.d(__webpack_exports__, "getMonth", function() { return _utils_date_getters__WEBPACK_IMPORTED_MODULE_2__["getMonth"]; });

/* harmony import */ var _create_local__WEBPACK_IMPORTED_MODULE_3__ = __webpack_require__(/*! ./create/local */ "./node_modules/ngx-bootstrap/chronos/create/local.js");
/* harmony reexport (safe) */ __webpack_require__.d(__webpack_exports__, "parseDate", function() { return _create_local__WEBPACK_IMPORTED_MODULE_3__["parseDate"]; });

/* harmony import */ var _format__WEBPACK_IMPORTED_MODULE_4__ = __webpack_require__(/*! ./format */ "./node_modules/ngx-bootstrap/chronos/format.js");
/* harmony reexport (safe) */ __webpack_require__.d(__webpack_exports__, "formatDate", function() { return _format__WEBPACK_IMPORTED_MODULE_4__["formatDate"]; });

/* harmony import */ var _locale_locales__WEBPACK_IMPORTED_MODULE_5__ = __webpack_require__(/*! ./locale/locales */ "./node_modules/ngx-bootstrap/chronos/locale/locales.js");
/* harmony reexport (safe) */ __webpack_require__.d(__webpack_exports__, "defineLocale", function() { return _locale_locales__WEBPACK_IMPORTED_MODULE_5__["defineLocale"]; });

/* harmony reexport (safe) */ __webpack_require__.d(__webpack_exports__, "getSetGlobalLocale", function() { return _locale_locales__WEBPACK_IMPORTED_MODULE_5__["getSetGlobalLocale"]; });

/* harmony reexport (safe) */ __webpack_require__.d(__webpack_exports__, "listLocales", function() { return _locale_locales__WEBPACK_IMPORTED_MODULE_5__["listLocales"]; });







//# sourceMappingURL=index.js.map

/***/ }),

/***/ "./node_modules/ngx-bootstrap/chronos/locale/calendar.js":
/*!***************************************************************!*\
  !*** ./node_modules/ngx-bootstrap/chronos/locale/calendar.js ***!
  \***************************************************************/
/*! exports provided: defaultCalendar */
/***/ (function(module, __webpack_exports__, __webpack_require__) {

"use strict";
__webpack_require__.r(__webpack_exports__);
/* harmony export (binding) */ __webpack_require__.d(__webpack_exports__, "defaultCalendar", function() { return defaultCalendar; });
var defaultCalendar = {
    sameDay: '[Today at] LT',
    nextDay: '[Tomorrow at] LT',
    nextWeek: 'dddd [at] LT',
    lastDay: '[Yesterday at] LT',
    lastWeek: '[Last] dddd [at] LT',
    sameElse: 'L'
};
//# sourceMappingURL=calendar.js.map

/***/ }),

/***/ "./node_modules/ngx-bootstrap/chronos/locale/locale.class.js":
/*!*******************************************************************!*\
  !*** ./node_modules/ngx-bootstrap/chronos/locale/locale.class.js ***!
  \*******************************************************************/
/*! exports provided: defaultLocaleMonths, defaultLocaleMonthsShort, defaultLocaleWeekdays, defaultLocaleWeekdaysShort, defaultLocaleWeekdaysMin, defaultLongDateFormat, defaultOrdinal, defaultDayOfMonthOrdinalParse, Locale */
/***/ (function(module, __webpack_exports__, __webpack_require__) {

"use strict";
__webpack_require__.r(__webpack_exports__);
/* harmony export (binding) */ __webpack_require__.d(__webpack_exports__, "defaultLocaleMonths", function() { return defaultLocaleMonths; });
/* harmony export (binding) */ __webpack_require__.d(__webpack_exports__, "defaultLocaleMonthsShort", function() { return defaultLocaleMonthsShort; });
/* harmony export (binding) */ __webpack_require__.d(__webpack_exports__, "defaultLocaleWeekdays", function() { return defaultLocaleWeekdays; });
/* harmony export (binding) */ __webpack_require__.d(__webpack_exports__, "defaultLocaleWeekdaysShort", function() { return defaultLocaleWeekdaysShort; });
/* harmony export (binding) */ __webpack_require__.d(__webpack_exports__, "defaultLocaleWeekdaysMin", function() { return defaultLocaleWeekdaysMin; });
/* harmony export (binding) */ __webpack_require__.d(__webpack_exports__, "defaultLongDateFormat", function() { return defaultLongDateFormat; });
/* harmony export (binding) */ __webpack_require__.d(__webpack_exports__, "defaultOrdinal", function() { return defaultOrdinal; });
/* harmony export (binding) */ __webpack_require__.d(__webpack_exports__, "defaultDayOfMonthOrdinalParse", function() { return defaultDayOfMonthOrdinalParse; });
/* harmony export (binding) */ __webpack_require__.d(__webpack_exports__, "Locale", function() { return Locale; });
/* harmony import */ var _units_week_calendar_utils__WEBPACK_IMPORTED_MODULE_0__ = __webpack_require__(/*! ../units/week-calendar-utils */ "./node_modules/ngx-bootstrap/chronos/units/week-calendar-utils.js");
/* harmony import */ var _utils_type_checks__WEBPACK_IMPORTED_MODULE_1__ = __webpack_require__(/*! ../utils/type-checks */ "./node_modules/ngx-bootstrap/chronos/utils/type-checks.js");
/* harmony import */ var _utils_date_getters__WEBPACK_IMPORTED_MODULE_2__ = __webpack_require__(/*! ../utils/date-getters */ "./node_modules/ngx-bootstrap/chronos/utils/date-getters.js");
/* harmony import */ var _parse_regex__WEBPACK_IMPORTED_MODULE_3__ = __webpack_require__(/*! ../parse/regex */ "./node_modules/ngx-bootstrap/chronos/parse/regex.js");
/* harmony import */ var _units_day_of_week__WEBPACK_IMPORTED_MODULE_4__ = __webpack_require__(/*! ../units/day-of-week */ "./node_modules/ngx-bootstrap/chronos/units/day-of-week.js");
// tslint:disable:max-file-line-count max-line-length cyclomatic-complexity





var MONTHS_IN_FORMAT = /D[oD]?(\[[^\[\]]*\]|\s)+MMMM?/;
var defaultLocaleMonths = 'January_February_March_April_May_June_July_August_September_October_November_December'.split('_');
var defaultLocaleMonthsShort = 'Jan_Feb_Mar_Apr_May_Jun_Jul_Aug_Sep_Oct_Nov_Dec'.split('_');
var defaultLocaleWeekdays = 'Sunday_Monday_Tuesday_Wednesday_Thursday_Friday_Saturday'.split('_');
var defaultLocaleWeekdaysShort = 'Sun_Mon_Tue_Wed_Thu_Fri_Sat'.split('_');
var defaultLocaleWeekdaysMin = 'Su_Mo_Tu_We_Th_Fr_Sa'.split('_');
var defaultLongDateFormat = {
    LTS: 'h:mm:ss A',
    LT: 'h:mm A',
    L: 'MM/DD/YYYY',
    LL: 'MMMM D, YYYY',
    LLL: 'MMMM D, YYYY h:mm A',
    LLLL: 'dddd, MMMM D, YYYY h:mm A'
};
var defaultOrdinal = '%d';
var defaultDayOfMonthOrdinalParse = /\d{1,2}/;
var defaultMonthsShortRegex = _parse_regex__WEBPACK_IMPORTED_MODULE_3__["matchWord"];
var defaultMonthsRegex = _parse_regex__WEBPACK_IMPORTED_MODULE_3__["matchWord"];
var Locale = /** @class */ (function () {
    function Locale(config) {
        if (!!config) {
            this.set(config);
        }
    }
    Locale.prototype.set = function (config) {
        var confKey;
        for (confKey in config) {
            if (!config.hasOwnProperty(confKey)) {
                continue;
            }
            var prop = config[confKey];
            var key = (Object(_utils_type_checks__WEBPACK_IMPORTED_MODULE_1__["isFunction"])(prop) ? confKey : "_" + confKey);
            this[key] = prop;
        }
        this._config = config;
    };
    Locale.prototype.calendar = function (key, date, now) {
        var output = this._calendar[key] || this._calendar.sameElse;
        return Object(_utils_type_checks__WEBPACK_IMPORTED_MODULE_1__["isFunction"])(output) ? output.call(null, date, now) : output;
    };
    Locale.prototype.longDateFormat = function (key) {
        var format = this._longDateFormat[key];
        var formatUpper = this._longDateFormat[key.toUpperCase()];
        if (format || !formatUpper) {
            return format;
        }
        this._longDateFormat[key] = formatUpper.replace(/MMMM|MM|DD|dddd/g, function (val) {
            return val.slice(1);
        });
        return this._longDateFormat[key];
    };
    Object.defineProperty(Locale.prototype, "invalidDate", {
        get: function () {
            return this._invalidDate;
        },
        set: function (val) {
            this._invalidDate = val;
        },
        enumerable: true,
        configurable: true
    });
    Locale.prototype.ordinal = function (num, token) {
        return this._ordinal.replace('%d', num.toString(10));
    };
    Locale.prototype.preparse = function (str) {
        return str;
    };
    Locale.prototype.postformat = function (str) {
        return str;
    };
    Locale.prototype.relativeTime = function (num, withoutSuffix, str, isFuture) {
        var output = this._relativeTime[str];
        return (Object(_utils_type_checks__WEBPACK_IMPORTED_MODULE_1__["isFunction"])(output)) ?
            output(num, withoutSuffix, str, isFuture) :
            output.replace(/%d/i, num.toString(10));
    };
    Locale.prototype.pastFuture = function (diff, output) {
        var format = this._relativeTime[diff > 0 ? 'future' : 'past'];
        return Object(_utils_type_checks__WEBPACK_IMPORTED_MODULE_1__["isFunction"])(format) ? format(output) : format.replace(/%s/i, output);
    };
    Locale.prototype.months = function (date, format, isUTC) {
        if (isUTC === void 0) { isUTC = false; }
        if (!date) {
            return Object(_utils_type_checks__WEBPACK_IMPORTED_MODULE_1__["isArray"])(this._months)
                ? this._months
                : this._months.standalone;
        }
        if (Object(_utils_type_checks__WEBPACK_IMPORTED_MODULE_1__["isArray"])(this._months)) {
            return this._months[Object(_utils_date_getters__WEBPACK_IMPORTED_MODULE_2__["getMonth"])(date, isUTC)];
        }
        var key = (this._months.isFormat || MONTHS_IN_FORMAT).test(format)
            ? 'format'
            : 'standalone';
        return this._months[key][Object(_utils_date_getters__WEBPACK_IMPORTED_MODULE_2__["getMonth"])(date, isUTC)];
    };
    Locale.prototype.monthsShort = function (date, format, isUTC) {
        if (isUTC === void 0) { isUTC = false; }
        if (!date) {
            return Object(_utils_type_checks__WEBPACK_IMPORTED_MODULE_1__["isArray"])(this._monthsShort)
                ? this._monthsShort
                : this._monthsShort.standalone;
        }
        if (Object(_utils_type_checks__WEBPACK_IMPORTED_MODULE_1__["isArray"])(this._monthsShort)) {
            return this._monthsShort[Object(_utils_date_getters__WEBPACK_IMPORTED_MODULE_2__["getMonth"])(date, isUTC)];
        }
        var key = MONTHS_IN_FORMAT.test(format) ? 'format' : 'standalone';
        return this._monthsShort[key][Object(_utils_date_getters__WEBPACK_IMPORTED_MODULE_2__["getMonth"])(date, isUTC)];
    };
    Locale.prototype.monthsParse = function (monthName, format, strict) {
        var date;
        var regex;
        if (this._monthsParseExact) {
            return this.handleMonthStrictParse(monthName, format, strict);
        }
        if (!this._monthsParse) {
            this._monthsParse = [];
            this._longMonthsParse = [];
            this._shortMonthsParse = [];
        }
        // TODO: add sorting
        // Sorting makes sure if one month (or abbr) is a prefix of another
        // see sorting in computeMonthsParse
        var i;
        for (i = 0; i < 12; i++) {
            // make the regex if we don't have it already
            date = new Date(Date.UTC(2000, i));
            if (strict && !this._longMonthsParse[i]) {
                var _months = this.months(date, '', true).replace('.', '');
                var _shortMonths = this.monthsShort(date, '', true).replace('.', '');
                this._longMonthsParse[i] = new RegExp("^" + _months + "$", 'i');
                this._shortMonthsParse[i] = new RegExp("^" + _shortMonths + "$", 'i');
            }
            if (!strict && !this._monthsParse[i]) {
                regex = "^" + this.months(date, '', true) + "|^" + this.monthsShort(date, '', true);
                this._monthsParse[i] = new RegExp(regex.replace('.', ''), 'i');
            }
            // test the regex
            if (strict && format === 'MMMM' && this._longMonthsParse[i].test(monthName)) {
                return i;
            }
            if (strict && format === 'MMM' && this._shortMonthsParse[i].test(monthName)) {
                return i;
            }
            if (!strict && this._monthsParse[i].test(monthName)) {
                return i;
            }
        }
    };
    Locale.prototype.monthsRegex = function (isStrict) {
        if (this._monthsParseExact) {
            if (!Object(_utils_type_checks__WEBPACK_IMPORTED_MODULE_1__["hasOwnProp"])(this, '_monthsRegex')) {
                this.computeMonthsParse();
            }
            if (isStrict) {
                return this._monthsStrictRegex;
            }
            return this._monthsRegex;
        }
        if (!Object(_utils_type_checks__WEBPACK_IMPORTED_MODULE_1__["hasOwnProp"])(this, '_monthsRegex')) {
            this._monthsRegex = defaultMonthsRegex;
        }
        return this._monthsStrictRegex && isStrict ?
            this._monthsStrictRegex : this._monthsRegex;
    };
    Locale.prototype.monthsShortRegex = function (isStrict) {
        if (this._monthsParseExact) {
            if (!Object(_utils_type_checks__WEBPACK_IMPORTED_MODULE_1__["hasOwnProp"])(this, '_monthsRegex')) {
                this.computeMonthsParse();
            }
            if (isStrict) {
                return this._monthsShortStrictRegex;
            }
            return this._monthsShortRegex;
        }
        if (!Object(_utils_type_checks__WEBPACK_IMPORTED_MODULE_1__["hasOwnProp"])(this, '_monthsShortRegex')) {
            this._monthsShortRegex = defaultMonthsShortRegex;
        }
        return this._monthsShortStrictRegex && isStrict ?
            this._monthsShortStrictRegex : this._monthsShortRegex;
    };
    /** Week */
    /** Week */
    Locale.prototype.week = /** Week */
    function (date, isUTC) {
        return Object(_units_week_calendar_utils__WEBPACK_IMPORTED_MODULE_0__["weekOfYear"])(date, this._week.dow, this._week.doy, isUTC).week;
    };
    Locale.prototype.firstDayOfWeek = function () {
        return this._week.dow;
    };
    Locale.prototype.firstDayOfYear = function () {
        return this._week.doy;
    };
    Locale.prototype.weekdays = function (date, format, isUTC) {
        if (!date) {
            return Object(_utils_type_checks__WEBPACK_IMPORTED_MODULE_1__["isArray"])(this._weekdays)
                ? this._weekdays
                : this._weekdays.standalone;
        }
        if (Object(_utils_type_checks__WEBPACK_IMPORTED_MODULE_1__["isArray"])(this._weekdays)) {
            return this._weekdays[Object(_utils_date_getters__WEBPACK_IMPORTED_MODULE_2__["getDay"])(date, isUTC)];
        }
        var _key = this._weekdays.isFormat.test(format)
            ? 'format'
            : 'standalone';
        return this._weekdays[_key][Object(_utils_date_getters__WEBPACK_IMPORTED_MODULE_2__["getDay"])(date, isUTC)];
    };
    Locale.prototype.weekdaysMin = function (date, format, isUTC) {
        return date ? this._weekdaysMin[Object(_utils_date_getters__WEBPACK_IMPORTED_MODULE_2__["getDay"])(date, isUTC)] : this._weekdaysMin;
    };
    Locale.prototype.weekdaysShort = function (date, format, isUTC) {
        return date ? this._weekdaysShort[Object(_utils_date_getters__WEBPACK_IMPORTED_MODULE_2__["getDay"])(date, isUTC)] : this._weekdaysShort;
    };
    // proto.weekdaysParse  =        localeWeekdaysParse;
    // proto.weekdaysParse  =        localeWeekdaysParse;
    Locale.prototype.weekdaysParse = 
    // proto.weekdaysParse  =        localeWeekdaysParse;
    function (weekdayName, format, strict) {
        var i;
        var regex;
        if (this._weekdaysParseExact) {
            return this.handleWeekStrictParse(weekdayName, format, strict);
        }
        if (!this._weekdaysParse) {
            this._weekdaysParse = [];
            this._minWeekdaysParse = [];
            this._shortWeekdaysParse = [];
            this._fullWeekdaysParse = [];
        }
        for (i = 0; i < 7; i++) {
            // make the regex if we don't have it already
            // fix: here is the issue
            var date = Object(_units_day_of_week__WEBPACK_IMPORTED_MODULE_4__["setDayOfWeek"])(new Date(Date.UTC(2000, 1)), i, null, true);
            if (strict && !this._fullWeekdaysParse[i]) {
                this._fullWeekdaysParse[i] = new RegExp("^" + this.weekdays(date, '', true).replace('.', '\.?') + "$", 'i');
                this._shortWeekdaysParse[i] = new RegExp("^" + this.weekdaysShort(date, '', true).replace('.', '\.?') + "$", 'i');
                this._minWeekdaysParse[i] = new RegExp("^" + this.weekdaysMin(date, '', true).replace('.', '\.?') + "$", 'i');
            }
            if (!this._weekdaysParse[i]) {
                regex = "^" + this.weekdays(date, '', true) + "|^" + this.weekdaysShort(date, '', true) + "|^" + this.weekdaysMin(date, '', true);
                this._weekdaysParse[i] = new RegExp(regex.replace('.', ''), 'i');
            }
            if (!Object(_utils_type_checks__WEBPACK_IMPORTED_MODULE_1__["isArray"])(this._fullWeekdaysParse)
                || !Object(_utils_type_checks__WEBPACK_IMPORTED_MODULE_1__["isArray"])(this._shortWeekdaysParse)
                || !Object(_utils_type_checks__WEBPACK_IMPORTED_MODULE_1__["isArray"])(this._minWeekdaysParse)
                || !Object(_utils_type_checks__WEBPACK_IMPORTED_MODULE_1__["isArray"])(this._weekdaysParse)) {
                return;
            }
            // test the regex
            if (strict && format === 'dddd' && this._fullWeekdaysParse[i].test(weekdayName)) {
                return i;
            }
            else if (strict && format === 'ddd' && this._shortWeekdaysParse[i].test(weekdayName)) {
                return i;
            }
            else if (strict && format === 'dd' && this._minWeekdaysParse[i].test(weekdayName)) {
                return i;
            }
            else if (!strict && this._weekdaysParse[i].test(weekdayName)) {
                return i;
            }
        }
    };
    // proto.weekdaysRegex       =        weekdaysRegex;
    // proto.weekdaysRegex       =        weekdaysRegex;
    Locale.prototype.weekdaysRegex = 
    // proto.weekdaysRegex       =        weekdaysRegex;
    function (isStrict) {
        if (this._weekdaysParseExact) {
            if (!Object(_utils_type_checks__WEBPACK_IMPORTED_MODULE_1__["hasOwnProp"])(this, '_weekdaysRegex')) {
                this.computeWeekdaysParse();
            }
            if (isStrict) {
                return this._weekdaysStrictRegex;
            }
            else {
                return this._weekdaysRegex;
            }
        }
        else {
            if (!Object(_utils_type_checks__WEBPACK_IMPORTED_MODULE_1__["hasOwnProp"])(this, '_weekdaysRegex')) {
                this._weekdaysRegex = _parse_regex__WEBPACK_IMPORTED_MODULE_3__["matchWord"];
            }
            return this._weekdaysStrictRegex && isStrict ?
                this._weekdaysStrictRegex : this._weekdaysRegex;
        }
    };
    // proto.weekdaysShortRegex  =        weekdaysShortRegex;
    // proto.weekdaysMinRegex    =        weekdaysMinRegex;
    // proto.weekdaysShortRegex  =        weekdaysShortRegex;
    // proto.weekdaysMinRegex    =        weekdaysMinRegex;
    Locale.prototype.weekdaysShortRegex = 
    // proto.weekdaysShortRegex  =        weekdaysShortRegex;
    // proto.weekdaysMinRegex    =        weekdaysMinRegex;
    function (isStrict) {
        if (this._weekdaysParseExact) {
            if (!Object(_utils_type_checks__WEBPACK_IMPORTED_MODULE_1__["hasOwnProp"])(this, '_weekdaysRegex')) {
                this.computeWeekdaysParse();
            }
            if (isStrict) {
                return this._weekdaysShortStrictRegex;
            }
            else {
                return this._weekdaysShortRegex;
            }
        }
        else {
            if (!Object(_utils_type_checks__WEBPACK_IMPORTED_MODULE_1__["hasOwnProp"])(this, '_weekdaysShortRegex')) {
                this._weekdaysShortRegex = _parse_regex__WEBPACK_IMPORTED_MODULE_3__["matchWord"];
            }
            return this._weekdaysShortStrictRegex && isStrict ?
                this._weekdaysShortStrictRegex : this._weekdaysShortRegex;
        }
    };
    Locale.prototype.weekdaysMinRegex = function (isStrict) {
        if (this._weekdaysParseExact) {
            if (!Object(_utils_type_checks__WEBPACK_IMPORTED_MODULE_1__["hasOwnProp"])(this, '_weekdaysRegex')) {
                this.computeWeekdaysParse();
            }
            if (isStrict) {
                return this._weekdaysMinStrictRegex;
            }
            else {
                return this._weekdaysMinRegex;
            }
        }
        else {
            if (!Object(_utils_type_checks__WEBPACK_IMPORTED_MODULE_1__["hasOwnProp"])(this, '_weekdaysMinRegex')) {
                this._weekdaysMinRegex = _parse_regex__WEBPACK_IMPORTED_MODULE_3__["matchWord"];
            }
            return this._weekdaysMinStrictRegex && isStrict ?
                this._weekdaysMinStrictRegex : this._weekdaysMinRegex;
        }
    };
    Locale.prototype.isPM = function (input) {
        // IE8 Quirks Mode & IE7 Standards Mode do not allow accessing strings like arrays
        // Using charAt should be more compatible.
        return input.toLowerCase().charAt(0) === 'p';
    };
    Locale.prototype.meridiem = function (hours, minutes, isLower) {
        if (hours > 11) {
            return isLower ? 'pm' : 'PM';
        }
        return isLower ? 'am' : 'AM';
    };
    Locale.prototype.formatLongDate = function (key) {
        this._longDateFormat = this._longDateFormat ? this._longDateFormat : defaultLongDateFormat;
        var format = this._longDateFormat[key];
        var formatUpper = this._longDateFormat[key.toUpperCase()];
        if (format || !formatUpper) {
            return format;
        }
        this._longDateFormat[key] = formatUpper.replace(/MMMM|MM|DD|dddd/g, function (val) {
            return val.slice(1);
        });
        return this._longDateFormat[key];
    };
    Locale.prototype.handleMonthStrictParse = function (monthName, format, strict) {
        var llc = monthName.toLocaleLowerCase();
        var i;
        var ii;
        var mom;
        if (!this._monthsParse) {
            // this is not used
            this._monthsParse = [];
            this._longMonthsParse = [];
            this._shortMonthsParse = [];
            for (i = 0; i < 12; ++i) {
                mom = new Date(2000, i);
                this._shortMonthsParse[i] = this.monthsShort(mom, '').toLocaleLowerCase();
                this._longMonthsParse[i] = this.months(mom, '').toLocaleLowerCase();
            }
        }
        if (strict) {
            if (format === 'MMM') {
                ii = this._shortMonthsParse.indexOf(llc);
                return ii !== -1 ? ii : null;
            }
            ii = this._longMonthsParse.indexOf(llc);
            return ii !== -1 ? ii : null;
        }
        if (format === 'MMM') {
            ii = this._shortMonthsParse.indexOf(llc);
            if (ii !== -1) {
                return ii;
            }
            ii = this._longMonthsParse.indexOf(llc);
            return ii !== -1 ? ii : null;
        }
        ii = this._longMonthsParse.indexOf(llc);
        if (ii !== -1) {
            return ii;
        }
        ii = this._shortMonthsParse.indexOf(llc);
        return ii !== -1 ? ii : null;
    };
    Locale.prototype.handleWeekStrictParse = function (weekdayName, format, strict) {
        var ii;
        var llc = weekdayName.toLocaleLowerCase();
        if (!this._weekdaysParse) {
            this._weekdaysParse = [];
            this._shortWeekdaysParse = [];
            this._minWeekdaysParse = [];
            var i = void 0;
            for (i = 0; i < 7; ++i) {
                var date = Object(_units_day_of_week__WEBPACK_IMPORTED_MODULE_4__["setDayOfWeek"])(new Date(Date.UTC(2000, 1)), i, null, true);
                this._minWeekdaysParse[i] = this.weekdaysMin(date).toLocaleLowerCase();
                this._shortWeekdaysParse[i] = this.weekdaysShort(date).toLocaleLowerCase();
                this._weekdaysParse[i] = this.weekdays(date, '').toLocaleLowerCase();
            }
        }
        if (!Object(_utils_type_checks__WEBPACK_IMPORTED_MODULE_1__["isArray"])(this._weekdaysParse)
            || !Object(_utils_type_checks__WEBPACK_IMPORTED_MODULE_1__["isArray"])(this._shortWeekdaysParse)
            || !Object(_utils_type_checks__WEBPACK_IMPORTED_MODULE_1__["isArray"])(this._minWeekdaysParse)) {
            return;
        }
        if (strict) {
            if (format === 'dddd') {
                ii = this._weekdaysParse.indexOf(llc);
                return ii !== -1 ? ii : null;
            }
            else if (format === 'ddd') {
                ii = this._shortWeekdaysParse.indexOf(llc);
                return ii !== -1 ? ii : null;
            }
            else {
                ii = this._minWeekdaysParse.indexOf(llc);
                return ii !== -1 ? ii : null;
            }
        }
        else {
            if (format === 'dddd') {
                ii = this._weekdaysParse.indexOf(llc);
                if (ii !== -1) {
                    return ii;
                }
                ii = this._shortWeekdaysParse.indexOf(llc);
                if (ii !== -1) {
                    return ii;
                }
                ii = this._minWeekdaysParse.indexOf(llc);
                return ii !== -1 ? ii : null;
            }
            else if (format === 'ddd') {
                ii = this._shortWeekdaysParse.indexOf(llc);
                if (ii !== -1) {
                    return ii;
                }
                ii = this._weekdaysParse.indexOf(llc);
                if (ii !== -1) {
                    return ii;
                }
                ii = this._minWeekdaysParse.indexOf(llc);
                return ii !== -1 ? ii : null;
            }
            else {
                ii = this._minWeekdaysParse.indexOf(llc);
                if (ii !== -1) {
                    return ii;
                }
                ii = this._weekdaysParse.indexOf(llc);
                if (ii !== -1) {
                    return ii;
                }
                ii = this._shortWeekdaysParse.indexOf(llc);
                return ii !== -1 ? ii : null;
            }
        }
    };
    Locale.prototype.computeMonthsParse = function () {
        var shortPieces = [];
        var longPieces = [];
        var mixedPieces = [];
        var date;
        var i;
        for (i = 0; i < 12; i++) {
            // make the regex if we don't have it already
            date = new Date(2000, i);
            shortPieces.push(this.monthsShort(date, ''));
            longPieces.push(this.months(date, ''));
            mixedPieces.push(this.months(date, ''));
            mixedPieces.push(this.monthsShort(date, ''));
        }
        // Sorting makes sure if one month (or abbr) is a prefix of another it
        // will match the longer piece.
        shortPieces.sort(cmpLenRev);
        longPieces.sort(cmpLenRev);
        mixedPieces.sort(cmpLenRev);
        for (i = 0; i < 12; i++) {
            shortPieces[i] = Object(_parse_regex__WEBPACK_IMPORTED_MODULE_3__["regexEscape"])(shortPieces[i]);
            longPieces[i] = Object(_parse_regex__WEBPACK_IMPORTED_MODULE_3__["regexEscape"])(longPieces[i]);
        }
        for (i = 0; i < 24; i++) {
            mixedPieces[i] = Object(_parse_regex__WEBPACK_IMPORTED_MODULE_3__["regexEscape"])(mixedPieces[i]);
        }
        this._monthsRegex = new RegExp("^(" + mixedPieces.join('|') + ")", 'i');
        this._monthsShortRegex = this._monthsRegex;
        this._monthsStrictRegex = new RegExp("^(" + longPieces.join('|') + ")", 'i');
        this._monthsShortStrictRegex = new RegExp("^(" + shortPieces.join('|') + ")", 'i');
    };
    Locale.prototype.computeWeekdaysParse = function () {
        var minPieces = [];
        var shortPieces = [];
        var longPieces = [];
        var mixedPieces = [];
        var i;
        for (i = 0; i < 7; i++) {
            // make the regex if we don't have it already
            // let mom = createUTC([2000, 1]).day(i);
            var date = Object(_units_day_of_week__WEBPACK_IMPORTED_MODULE_4__["setDayOfWeek"])(new Date(Date.UTC(2000, 1)), i, null, true);
            var minp = this.weekdaysMin(date);
            var shortp = this.weekdaysShort(date);
            var longp = this.weekdays(date);
            minPieces.push(minp);
            shortPieces.push(shortp);
            longPieces.push(longp);
            mixedPieces.push(minp);
            mixedPieces.push(shortp);
            mixedPieces.push(longp);
        }
        // Sorting makes sure if one weekday (or abbr) is a prefix of another it
        // will match the longer piece.
        minPieces.sort(cmpLenRev);
        shortPieces.sort(cmpLenRev);
        longPieces.sort(cmpLenRev);
        mixedPieces.sort(cmpLenRev);
        for (i = 0; i < 7; i++) {
            shortPieces[i] = Object(_parse_regex__WEBPACK_IMPORTED_MODULE_3__["regexEscape"])(shortPieces[i]);
            longPieces[i] = Object(_parse_regex__WEBPACK_IMPORTED_MODULE_3__["regexEscape"])(longPieces[i]);
            mixedPieces[i] = Object(_parse_regex__WEBPACK_IMPORTED_MODULE_3__["regexEscape"])(mixedPieces[i]);
        }
        this._weekdaysRegex = new RegExp("^(" + mixedPieces.join('|') + ")", 'i');
        this._weekdaysShortRegex = this._weekdaysRegex;
        this._weekdaysMinRegex = this._weekdaysRegex;
        this._weekdaysStrictRegex = new RegExp("^(" + longPieces.join('|') + ")", 'i');
        this._weekdaysShortStrictRegex = new RegExp("^(" + shortPieces.join('|') + ")", 'i');
        this._weekdaysMinStrictRegex = new RegExp("^(" + minPieces.join('|') + ")", 'i');
    };
    return Locale;
}());

function cmpLenRev(a, b) {
    return b.length - a.length;
}
//# sourceMappingURL=locale.class.js.map

/***/ }),

/***/ "./node_modules/ngx-bootstrap/chronos/locale/locale.defaults.js":
/*!**********************************************************************!*\
  !*** ./node_modules/ngx-bootstrap/chronos/locale/locale.defaults.js ***!
  \**********************************************************************/
/*! exports provided: defaultInvalidDate, defaultLocaleWeek, defaultLocaleMeridiemParse, defaultRelativeTime, baseConfig */
/***/ (function(module, __webpack_exports__, __webpack_require__) {

"use strict";
__webpack_require__.r(__webpack_exports__);
/* harmony export (binding) */ __webpack_require__.d(__webpack_exports__, "defaultInvalidDate", function() { return defaultInvalidDate; });
/* harmony export (binding) */ __webpack_require__.d(__webpack_exports__, "defaultLocaleWeek", function() { return defaultLocaleWeek; });
/* harmony export (binding) */ __webpack_require__.d(__webpack_exports__, "defaultLocaleMeridiemParse", function() { return defaultLocaleMeridiemParse; });
/* harmony export (binding) */ __webpack_require__.d(__webpack_exports__, "defaultRelativeTime", function() { return defaultRelativeTime; });
/* harmony export (binding) */ __webpack_require__.d(__webpack_exports__, "baseConfig", function() { return baseConfig; });
/* harmony import */ var _locale_class__WEBPACK_IMPORTED_MODULE_0__ = __webpack_require__(/*! ./locale.class */ "./node_modules/ngx-bootstrap/chronos/locale/locale.class.js");
/* harmony import */ var _calendar__WEBPACK_IMPORTED_MODULE_1__ = __webpack_require__(/*! ./calendar */ "./node_modules/ngx-bootstrap/chronos/locale/calendar.js");


var defaultInvalidDate = 'Invalid date';
var defaultLocaleWeek = {
    dow: 0,
    // Sunday is the first day of the week.
    doy: 6 // The week that contains Jan 1st is the first week of the year.
};
var defaultLocaleMeridiemParse = /[ap]\.?m?\.?/i;
var defaultRelativeTime = {
    future: 'in %s',
    past: '%s ago',
    s: 'a few seconds',
    ss: '%d seconds',
    m: 'a minute',
    mm: '%d minutes',
    h: 'an hour',
    hh: '%d hours',
    d: 'a day',
    dd: '%d days',
    M: 'a month',
    MM: '%d months',
    y: 'a year',
    yy: '%d years'
};
var baseConfig = {
    calendar: _calendar__WEBPACK_IMPORTED_MODULE_1__["defaultCalendar"],
    longDateFormat: _locale_class__WEBPACK_IMPORTED_MODULE_0__["defaultLongDateFormat"],
    invalidDate: defaultInvalidDate,
    ordinal: _locale_class__WEBPACK_IMPORTED_MODULE_0__["defaultOrdinal"],
    dayOfMonthOrdinalParse: _locale_class__WEBPACK_IMPORTED_MODULE_0__["defaultDayOfMonthOrdinalParse"],
    relativeTime: defaultRelativeTime,
    months: _locale_class__WEBPACK_IMPORTED_MODULE_0__["defaultLocaleMonths"],
    monthsShort: _locale_class__WEBPACK_IMPORTED_MODULE_0__["defaultLocaleMonthsShort"],
    week: defaultLocaleWeek,
    weekdays: _locale_class__WEBPACK_IMPORTED_MODULE_0__["defaultLocaleWeekdays"],
    weekdaysMin: _locale_class__WEBPACK_IMPORTED_MODULE_0__["defaultLocaleWeekdaysMin"],
    weekdaysShort: _locale_class__WEBPACK_IMPORTED_MODULE_0__["defaultLocaleWeekdaysShort"],
    meridiemParse: defaultLocaleMeridiemParse
};
//# sourceMappingURL=locale.defaults.js.map

/***/ }),

/***/ "./node_modules/ngx-bootstrap/chronos/locale/locales.js":
/*!**************************************************************!*\
  !*** ./node_modules/ngx-bootstrap/chronos/locale/locales.js ***!
  \**************************************************************/
/*! exports provided: mergeConfigs, getSetGlobalLocale, defineLocale, updateLocale, getLocale, listLocales */
/***/ (function(module, __webpack_exports__, __webpack_require__) {

"use strict";
__webpack_require__.r(__webpack_exports__);
/* harmony export (binding) */ __webpack_require__.d(__webpack_exports__, "mergeConfigs", function() { return mergeConfigs; });
/* harmony export (binding) */ __webpack_require__.d(__webpack_exports__, "getSetGlobalLocale", function() { return getSetGlobalLocale; });
/* harmony export (binding) */ __webpack_require__.d(__webpack_exports__, "defineLocale", function() { return defineLocale; });
/* harmony export (binding) */ __webpack_require__.d(__webpack_exports__, "updateLocale", function() { return updateLocale; });
/* harmony export (binding) */ __webpack_require__.d(__webpack_exports__, "getLocale", function() { return getLocale; });
/* harmony export (binding) */ __webpack_require__.d(__webpack_exports__, "listLocales", function() { return listLocales; });
/* harmony import */ var _locale_class__WEBPACK_IMPORTED_MODULE_0__ = __webpack_require__(/*! ./locale.class */ "./node_modules/ngx-bootstrap/chronos/locale/locale.class.js");
/* harmony import */ var _locale_defaults__WEBPACK_IMPORTED_MODULE_1__ = __webpack_require__(/*! ./locale.defaults */ "./node_modules/ngx-bootstrap/chronos/locale/locale.defaults.js");
/* harmony import */ var _utils_type_checks__WEBPACK_IMPORTED_MODULE_2__ = __webpack_require__(/*! ../utils/type-checks */ "./node_modules/ngx-bootstrap/chronos/utils/type-checks.js");
/* harmony import */ var _utils_compare_arrays__WEBPACK_IMPORTED_MODULE_3__ = __webpack_require__(/*! ../utils/compare-arrays */ "./node_modules/ngx-bootstrap/chronos/utils/compare-arrays.js");




var locales = {};
var localeFamilies = {};
var globalLocale;
function normalizeLocale(key) {
    return key ? key.toLowerCase().replace('_', '-') : key;
}
// pick the locale from the array
// try ['en-au', 'en-gb'] as 'en-au', 'en-gb', 'en', as in move through the list trying each
// substring from most specific to least,
// but move to the next array item if it's a more specific variant than the current root
function chooseLocale(names) {
    var next;
    var locale;
    var i = 0;
    while (i < names.length) {
        var split = normalizeLocale(names[i]).split('-');
        var j = split.length;
        next = normalizeLocale(names[i + 1]);
        next = next ? next.split('-') : null;
        while (j > 0) {
            locale = loadLocale(split.slice(0, j).join('-'));
            if (locale) {
                return locale;
            }
            if (next && next.length >= j && Object(_utils_compare_arrays__WEBPACK_IMPORTED_MODULE_3__["compareArrays"])(split, next, true) >= j - 1) {
                // the next array item is better than a shallower substring of this one
                break;
            }
            j--;
        }
        i++;
    }
    return null;
}
function mergeConfigs(parentConfig, childConfig) {
    var res = Object.assign({}, parentConfig);
    for (var childProp in childConfig) {
        if (!Object(_utils_type_checks__WEBPACK_IMPORTED_MODULE_2__["hasOwnProp"])(childConfig, childProp)) {
            continue;
        }
        if (Object(_utils_type_checks__WEBPACK_IMPORTED_MODULE_2__["isObject"])(parentConfig[childProp]) && Object(_utils_type_checks__WEBPACK_IMPORTED_MODULE_2__["isObject"])(childConfig[childProp])) {
            res[childProp] = {};
            Object.assign(res[childProp], parentConfig[childProp]);
            Object.assign(res[childProp], childConfig[childProp]);
        }
        else if (childConfig[childProp] != null) {
            res[childProp] = childConfig[childProp];
        }
        else {
            delete res[childProp];
        }
    }
    var parentProp;
    for (parentProp in parentConfig) {
        if (Object(_utils_type_checks__WEBPACK_IMPORTED_MODULE_2__["hasOwnProp"])(parentConfig, parentProp) &&
            !Object(_utils_type_checks__WEBPACK_IMPORTED_MODULE_2__["hasOwnProp"])(childConfig, parentProp) &&
            Object(_utils_type_checks__WEBPACK_IMPORTED_MODULE_2__["isObject"])(parentConfig[parentProp])) {
            // make sure changes to properties don't modify parent config
            res[parentProp] = Object.assign({}, res[parentProp]);
        }
    }
    return res;
}
function loadLocale(name) {
    // no way!
    /* var oldLocale = null;
       // TODO: Find a better way to register and load all the locales in Node
       if (!locales[name] && (typeof module !== 'undefined') &&
         module && module.exports) {
         try {
           oldLocale = globalLocale._abbr;
           var aliasedRequire = require;
           aliasedRequire('./locale/' + name);
           getSetGlobalLocale(oldLocale);
         } catch (e) {}
       }*/
    if (!locales[name]) {
        // tslint:disable-next-line
        console.error("Khronos locale error: please load locale \"" + name + "\" before using it");
        // throw new Error(`Khronos locale error: please load locale "${name}" before using it`);
    }
    return locales[name];
}
// This function will load locale and then set the global locale.  If
// no arguments are passed in, it will simply return the current global
// locale key.
function getSetGlobalLocale(key, values) {
    var data;
    if (key) {
        if (Object(_utils_type_checks__WEBPACK_IMPORTED_MODULE_2__["isUndefined"])(values)) {
            data = getLocale(key);
        }
        else if (Object(_utils_type_checks__WEBPACK_IMPORTED_MODULE_2__["isString"])(key)) {
            data = defineLocale(key, values);
        }
        if (data) {
            globalLocale = data;
        }
    }
    return globalLocale && globalLocale._abbr;
}
function defineLocale(name, config) {
    if (config === null) {
        // useful for testing
        delete locales[name];
        globalLocale = getLocale('en');
        return null;
    }
    if (!config) {
        return;
    }
    var parentConfig = _locale_defaults__WEBPACK_IMPORTED_MODULE_1__["baseConfig"];
    config.abbr = name;
    if (config.parentLocale != null) {
        if (locales[config.parentLocale] != null) {
            parentConfig = locales[config.parentLocale]._config;
        }
        else {
            if (!localeFamilies[config.parentLocale]) {
                localeFamilies[config.parentLocale] = [];
            }
            localeFamilies[config.parentLocale].push({ name: name, config: config });
            return null;
        }
    }
    locales[name] = new _locale_class__WEBPACK_IMPORTED_MODULE_0__["Locale"](mergeConfigs(parentConfig, config));
    if (localeFamilies[name]) {
        localeFamilies[name].forEach(function (x) {
            defineLocale(x.name, x.config);
        });
    }
    // backwards compat for now: also set the locale
    // make sure we set the locale AFTER all child locales have been
    // created, so we won't end up with the child locale set.
    getSetGlobalLocale(name);
    return locales[name];
}
function updateLocale(name, config) {
    var _config = config;
    if (_config != null) {
        var parentConfig = _locale_defaults__WEBPACK_IMPORTED_MODULE_1__["baseConfig"];
        // MERGE
        var tmpLocale = loadLocale(name);
        if (tmpLocale != null) {
            parentConfig = tmpLocale._config;
        }
        _config = mergeConfigs(parentConfig, _config);
        var locale = new _locale_class__WEBPACK_IMPORTED_MODULE_0__["Locale"](_config);
        locale.parentLocale = locales[name];
        locales[name] = locale;
        // backwards compat for now: also set the locale
        getSetGlobalLocale(name);
    }
    else {
        // pass null for config to unupdate, useful for tests
        if (locales[name] != null) {
            if (locales[name].parentLocale != null) {
                locales[name] = locales[name].parentLocale;
            }
            else if (locales[name] != null) {
                delete locales[name];
            }
        }
    }
    return locales[name];
}
// returns locale data
function getLocale(key) {
    if (!key) {
        return globalLocale;
    }
    // let locale;
    var _key = Object(_utils_type_checks__WEBPACK_IMPORTED_MODULE_2__["isArray"])(key) ? key : [key];
    return chooseLocale(_key);
}
function listLocales() {
    return Object.keys(locales);
}
// define default locale
getSetGlobalLocale('en', {
    dayOfMonthOrdinalParse: /\d{1,2}(th|st|nd|rd)/,
    ordinal: function (num) {
        var b = num % 10;
        var output = Object(_utils_type_checks__WEBPACK_IMPORTED_MODULE_2__["toInt"])((num % 100) / 10) === 1
            ? 'th'
            : b === 1 ? 'st' : b === 2 ? 'nd' : b === 3 ? 'rd' : 'th';
        return num + output;
    }
});
//# sourceMappingURL=locales.js.map

/***/ }),

/***/ "./node_modules/ngx-bootstrap/chronos/moment/add-subtract.js":
/*!*******************************************************************!*\
  !*** ./node_modules/ngx-bootstrap/chronos/moment/add-subtract.js ***!
  \*******************************************************************/
/*! exports provided: add, subtract, addSubtract */
/***/ (function(module, __webpack_exports__, __webpack_require__) {

"use strict";
__webpack_require__.r(__webpack_exports__);
/* harmony export (binding) */ __webpack_require__.d(__webpack_exports__, "add", function() { return add; });
/* harmony export (binding) */ __webpack_require__.d(__webpack_exports__, "subtract", function() { return subtract; });
/* harmony export (binding) */ __webpack_require__.d(__webpack_exports__, "addSubtract", function() { return addSubtract; });
/* harmony import */ var _duration_create__WEBPACK_IMPORTED_MODULE_0__ = __webpack_require__(/*! ../duration/create */ "./node_modules/ngx-bootstrap/chronos/duration/create.js");
/* harmony import */ var _utils_abs_round__WEBPACK_IMPORTED_MODULE_1__ = __webpack_require__(/*! ../utils/abs-round */ "./node_modules/ngx-bootstrap/chronos/utils/abs-round.js");
/* harmony import */ var _utils_date_getters__WEBPACK_IMPORTED_MODULE_2__ = __webpack_require__(/*! ../utils/date-getters */ "./node_modules/ngx-bootstrap/chronos/utils/date-getters.js");
/* harmony import */ var _utils_date_setters__WEBPACK_IMPORTED_MODULE_3__ = __webpack_require__(/*! ../utils/date-setters */ "./node_modules/ngx-bootstrap/chronos/utils/date-setters.js");
/* harmony import */ var _create_clone__WEBPACK_IMPORTED_MODULE_4__ = __webpack_require__(/*! ../create/clone */ "./node_modules/ngx-bootstrap/chronos/create/clone.js");





function add(date, val, period, isUTC) {
    var dur = Object(_duration_create__WEBPACK_IMPORTED_MODULE_0__["createDuration"])(val, period);
    return addSubtract(date, dur, 1, isUTC);
}
function subtract(date, val, period, isUTC) {
    var dur = Object(_duration_create__WEBPACK_IMPORTED_MODULE_0__["createDuration"])(val, period);
    return addSubtract(date, dur, -1, isUTC);
}
function addSubtract(date, duration, isAdding, isUTC) {
    var milliseconds = duration._milliseconds;
    var days = Object(_utils_abs_round__WEBPACK_IMPORTED_MODULE_1__["absRound"])(duration._days);
    var months = Object(_utils_abs_round__WEBPACK_IMPORTED_MODULE_1__["absRound"])(duration._months);
    // todo: add timezones support
    // const _updateOffset = updateOffset == null ? true : updateOffset;
    if (months) {
        Object(_utils_date_setters__WEBPACK_IMPORTED_MODULE_3__["setMonth"])(date, Object(_utils_date_getters__WEBPACK_IMPORTED_MODULE_2__["getMonth"])(date, isUTC) + months * isAdding, isUTC);
    }
    if (days) {
        Object(_utils_date_setters__WEBPACK_IMPORTED_MODULE_3__["setDate"])(date, Object(_utils_date_getters__WEBPACK_IMPORTED_MODULE_2__["getDate"])(date, isUTC) + days * isAdding, isUTC);
    }
    if (milliseconds) {
        Object(_utils_date_setters__WEBPACK_IMPORTED_MODULE_3__["setTime"])(date, Object(_utils_date_getters__WEBPACK_IMPORTED_MODULE_2__["getTime"])(date) + milliseconds * isAdding);
    }
    return Object(_create_clone__WEBPACK_IMPORTED_MODULE_4__["cloneDate"])(date);
    // todo: add timezones support
    // if (_updateOffset) {
    //   hooks.updateOffset(date, days || months);
    // }
}
//# sourceMappingURL=add-subtract.js.map

/***/ }),

/***/ "./node_modules/ngx-bootstrap/chronos/moment/calendar.js":
/*!***************************************************************!*\
  !*** ./node_modules/ngx-bootstrap/chronos/moment/calendar.js ***!
  \***************************************************************/
/*! exports provided: getCalendarFormat, calendar */
/***/ (function(module, __webpack_exports__, __webpack_require__) {

"use strict";
__webpack_require__.r(__webpack_exports__);
/* harmony export (binding) */ __webpack_require__.d(__webpack_exports__, "getCalendarFormat", function() { return getCalendarFormat; });
/* harmony export (binding) */ __webpack_require__.d(__webpack_exports__, "calendar", function() { return calendar; });
/* harmony import */ var _diff__WEBPACK_IMPORTED_MODULE_0__ = __webpack_require__(/*! ./diff */ "./node_modules/ngx-bootstrap/chronos/moment/diff.js");
/* harmony import */ var _units_offset__WEBPACK_IMPORTED_MODULE_1__ = __webpack_require__(/*! ../units/offset */ "./node_modules/ngx-bootstrap/chronos/units/offset.js");
/* harmony import */ var _utils_type_checks__WEBPACK_IMPORTED_MODULE_2__ = __webpack_require__(/*! ../utils/type-checks */ "./node_modules/ngx-bootstrap/chronos/utils/type-checks.js");
/* harmony import */ var _create_clone__WEBPACK_IMPORTED_MODULE_3__ = __webpack_require__(/*! ../create/clone */ "./node_modules/ngx-bootstrap/chronos/create/clone.js");
/* harmony import */ var _utils_start_end_of__WEBPACK_IMPORTED_MODULE_4__ = __webpack_require__(/*! ../utils/start-end-of */ "./node_modules/ngx-bootstrap/chronos/utils/start-end-of.js");
/* harmony import */ var _format__WEBPACK_IMPORTED_MODULE_5__ = __webpack_require__(/*! ../format */ "./node_modules/ngx-bootstrap/chronos/format.js");
/* harmony import */ var _locale_locales__WEBPACK_IMPORTED_MODULE_6__ = __webpack_require__(/*! ../locale/locales */ "./node_modules/ngx-bootstrap/chronos/locale/locales.js");







function getCalendarFormat(date, now, config) {
    var _diff = Object(_diff__WEBPACK_IMPORTED_MODULE_0__["diff"])(date, now, 'day', true, config);
    switch (true) {
        case _diff < -6: return 'sameElse';
        case _diff < -1: return 'lastWeek';
        case _diff < 0: return 'lastDay';
        case _diff < 1: return 'sameDay';
        case _diff < 2: return 'nextDay';
        case _diff < 7: return 'nextWeek';
        default: return 'sameElse';
    }
}
function calendar(date, time, formats, locale, config) {
    if (locale === void 0) { locale = Object(_locale_locales__WEBPACK_IMPORTED_MODULE_6__["getLocale"])(); }
    if (config === void 0) { config = {}; }
    // We want to compare the start of today, vs this.
    // Getting start-of-today depends on whether we're local/utc/offset or not.
    var now = time;
    var sod = Object(_utils_start_end_of__WEBPACK_IMPORTED_MODULE_4__["startOf"])(Object(_units_offset__WEBPACK_IMPORTED_MODULE_1__["cloneWithOffset"])(now, date, config), 'day', config._isUTC);
    var format = getCalendarFormat(date, sod, { _isUTC: true, _offset: 0 }) || 'sameElse';
    var output;
    if (formats) {
        var _format = formats[format];
        if (Object(_utils_type_checks__WEBPACK_IMPORTED_MODULE_2__["isString"])(_format)) {
            output = _format;
        }
        if (Object(_utils_type_checks__WEBPACK_IMPORTED_MODULE_2__["isFunction"])(_format)) {
            output = _format.call(null, date, now);
        }
    }
    if (!output) {
        output = locale.calendar(format, date, Object(_create_clone__WEBPACK_IMPORTED_MODULE_3__["cloneDate"])(now));
    }
    return Object(_format__WEBPACK_IMPORTED_MODULE_5__["formatDate"])(date, output, config._locale._abbr, config._isUTC, config._offset);
}
//# sourceMappingURL=calendar.js.map

/***/ }),

/***/ "./node_modules/ngx-bootstrap/chronos/moment/diff.js":
/*!***********************************************************!*\
  !*** ./node_modules/ngx-bootstrap/chronos/moment/diff.js ***!
  \***********************************************************/
/*! exports provided: diff */
/***/ (function(module, __webpack_exports__, __webpack_require__) {

"use strict";
__webpack_require__.r(__webpack_exports__);
/* harmony export (binding) */ __webpack_require__.d(__webpack_exports__, "diff", function() { return diff; });
/* harmony import */ var _units_offset__WEBPACK_IMPORTED_MODULE_0__ = __webpack_require__(/*! ../units/offset */ "./node_modules/ngx-bootstrap/chronos/units/offset.js");
/* harmony import */ var _utils__WEBPACK_IMPORTED_MODULE_1__ = __webpack_require__(/*! ../utils */ "./node_modules/ngx-bootstrap/chronos/utils.js");
/* harmony import */ var _utils_type_checks__WEBPACK_IMPORTED_MODULE_2__ = __webpack_require__(/*! ../utils/type-checks */ "./node_modules/ngx-bootstrap/chronos/utils/type-checks.js");
/* harmony import */ var _utils_date_getters__WEBPACK_IMPORTED_MODULE_3__ = __webpack_require__(/*! ../utils/date-getters */ "./node_modules/ngx-bootstrap/chronos/utils/date-getters.js");
/* harmony import */ var _add_subtract__WEBPACK_IMPORTED_MODULE_4__ = __webpack_require__(/*! ./add-subtract */ "./node_modules/ngx-bootstrap/chronos/moment/add-subtract.js");
/* harmony import */ var _create_clone__WEBPACK_IMPORTED_MODULE_5__ = __webpack_require__(/*! ../create/clone */ "./node_modules/ngx-bootstrap/chronos/create/clone.js");






function diff(date, input, units, asFloat, config) {
    if (config === void 0) { config = {}; }
    if (!Object(_utils_type_checks__WEBPACK_IMPORTED_MODULE_2__["isDateValid"])(date)) {
        return NaN;
    }
    var that = Object(_units_offset__WEBPACK_IMPORTED_MODULE_0__["cloneWithOffset"])(input, date, config);
    if (!Object(_utils_type_checks__WEBPACK_IMPORTED_MODULE_2__["isDateValid"])(that)) {
        return NaN;
    }
    // const zoneDelta = (getUTCOffset(input, dateConfig) - getUTCOffset(date, dateConfig)) * 6e4;
    var zoneDelta = Object(_utils_type_checks__WEBPACK_IMPORTED_MODULE_2__["isNumber"])(config._zoneDelta)
        ? config._zoneDelta * 6e4
        : (Object(_units_offset__WEBPACK_IMPORTED_MODULE_0__["getUTCOffset"])(input, config) - Object(_units_offset__WEBPACK_IMPORTED_MODULE_0__["getUTCOffset"])(date, config)) * 6e4;
    var output;
    switch (units) {
        case 'year':
            output = monthDiff(date, that) / 12;
            break;
        case 'month':
            output = monthDiff(date, that);
            break;
        case 'quarter':
            output = monthDiff(date, that) / 3;
            break;
        case 'seconds':
            output = (date.valueOf() - that.valueOf()) / 1e3;
            break; // 1000
        case 'minutes':
            output = (date.valueOf() - that.valueOf()) / 6e4;
            break; // 1000 * 60
        case 'hours':
            output = (date.valueOf() - that.valueOf()) / 36e5;
            break; // 1000 * 60 * 60
        case 'day':
            output = (date.valueOf() - that.valueOf() - zoneDelta) / 864e5;
            break; // 1000 * 60 * 60 * 24, negate dst
        case 'week':
            output = (date.valueOf() - that.valueOf() - zoneDelta) / 6048e5;
            break; // 1000 * 60 * 60 * 24 * 7, negate dst
        default:
            output = date.valueOf() - that.valueOf();
    }
    return asFloat ? output : Object(_utils__WEBPACK_IMPORTED_MODULE_1__["absFloor"])(output);
}
function monthDiff(a, b) {
    // difference in months
    var wholeMonthDiff = ((Object(_utils_date_getters__WEBPACK_IMPORTED_MODULE_3__["getFullYear"])(b) - Object(_utils_date_getters__WEBPACK_IMPORTED_MODULE_3__["getFullYear"])(a)) * 12) + (Object(_utils_date_getters__WEBPACK_IMPORTED_MODULE_3__["getMonth"])(b) - Object(_utils_date_getters__WEBPACK_IMPORTED_MODULE_3__["getMonth"])(a));
    // b is in (anchor - 1 month, anchor + 1 month)
    var anchor = Object(_add_subtract__WEBPACK_IMPORTED_MODULE_4__["add"])(Object(_create_clone__WEBPACK_IMPORTED_MODULE_5__["cloneDate"])(a), wholeMonthDiff, 'month');
    var anchor2;
    var adjust;
    if (b.valueOf() - anchor.valueOf() < 0) {
        anchor2 = Object(_add_subtract__WEBPACK_IMPORTED_MODULE_4__["add"])(Object(_create_clone__WEBPACK_IMPORTED_MODULE_5__["cloneDate"])(a), wholeMonthDiff - 1, 'month');
        // linear across the month
        adjust = (b.valueOf() - anchor.valueOf()) / (anchor.valueOf() - anchor2.valueOf());
    }
    else {
        anchor2 = Object(_add_subtract__WEBPACK_IMPORTED_MODULE_4__["add"])(Object(_create_clone__WEBPACK_IMPORTED_MODULE_5__["cloneDate"])(a), wholeMonthDiff + 1, 'month');
        // linear across the month
        adjust = (b.valueOf() - anchor.valueOf()) / (anchor2.valueOf() - anchor.valueOf());
    }
    // check for negative zero, return zero if negative zero
    return -(wholeMonthDiff + adjust) || 0;
}
//# sourceMappingURL=diff.js.map

/***/ }),

/***/ "./node_modules/ngx-bootstrap/chronos/moment/min-max.js":
/*!**************************************************************!*\
  !*** ./node_modules/ngx-bootstrap/chronos/moment/min-max.js ***!
  \**************************************************************/
/*! exports provided: min, max */
/***/ (function(module, __webpack_exports__, __webpack_require__) {

"use strict";
__webpack_require__.r(__webpack_exports__);
/* harmony export (binding) */ __webpack_require__.d(__webpack_exports__, "min", function() { return min; });
/* harmony export (binding) */ __webpack_require__.d(__webpack_exports__, "max", function() { return max; });
/* harmony import */ var _utils_type_checks__WEBPACK_IMPORTED_MODULE_0__ = __webpack_require__(/*! ../utils/type-checks */ "./node_modules/ngx-bootstrap/chronos/utils/type-checks.js");
/* harmony import */ var _utils_date_compare__WEBPACK_IMPORTED_MODULE_1__ = __webpack_require__(/*! ../utils/date-compare */ "./node_modules/ngx-bootstrap/chronos/utils/date-compare.js");


function pickBy(fn, dates) {
    var _dates;
    var _firstArg = dates[0];
    if (Object(_utils_type_checks__WEBPACK_IMPORTED_MODULE_0__["isArray"])(_firstArg) && dates.length === 1) {
        _dates = _firstArg;
    }
    else if (Object(_utils_type_checks__WEBPACK_IMPORTED_MODULE_0__["isArray"])(dates)) {
        _dates = dates;
    }
    if (!_dates || !_dates.length) {
        return new Date();
    }
    var res = _dates[0];
    for (var i = 1; i < _dates.length; ++i) {
        // if (!moments[i].isValid() || moments[i][fn](res)) {
        if (!Object(_utils_type_checks__WEBPACK_IMPORTED_MODULE_0__["isDateValid"])(_dates[i]) || fn.call(null, _dates[i], res)) {
            res = _dates[i];
        }
    }
    return res;
}
// TODO: Use [].sort instead?
function min() {
    var args = [];
    for (var _i = 0; _i < arguments.length; _i++) {
        args[_i] = arguments[_i];
    }
    // const args = [].slice.call(arguments, 0);
    return pickBy(_utils_date_compare__WEBPACK_IMPORTED_MODULE_1__["isBefore"], args);
}
function max() {
    var args = [];
    for (var _i = 0; _i < arguments.length; _i++) {
        args[_i] = arguments[_i];
    }
    // const args = [].slice.call(arguments, 0);
    return pickBy(_utils_date_compare__WEBPACK_IMPORTED_MODULE_1__["isAfter"], args);
}
//# sourceMappingURL=min-max.js.map

/***/ }),

/***/ "./node_modules/ngx-bootstrap/chronos/parse/regex.js":
/*!***********************************************************!*\
  !*** ./node_modules/ngx-bootstrap/chronos/parse/regex.js ***!
  \***********************************************************/
/*! exports provided: match1, match2, match3, match4, match6, match1to2, match3to4, match5to6, match1to3, match1to4, match1to6, matchUnsigned, matchSigned, matchOffset, matchShortOffset, matchTimestamp, matchWord, addRegexToken, getParseRegexForToken, regexEscape */
/***/ (function(module, __webpack_exports__, __webpack_require__) {

"use strict";
__webpack_require__.r(__webpack_exports__);
/* harmony export (binding) */ __webpack_require__.d(__webpack_exports__, "match1", function() { return match1; });
/* harmony export (binding) */ __webpack_require__.d(__webpack_exports__, "match2", function() { return match2; });
/* harmony export (binding) */ __webpack_require__.d(__webpack_exports__, "match3", function() { return match3; });
/* harmony export (binding) */ __webpack_require__.d(__webpack_exports__, "match4", function() { return match4; });
/* harmony export (binding) */ __webpack_require__.d(__webpack_exports__, "match6", function() { return match6; });
/* harmony export (binding) */ __webpack_require__.d(__webpack_exports__, "match1to2", function() { return match1to2; });
/* harmony export (binding) */ __webpack_require__.d(__webpack_exports__, "match3to4", function() { return match3to4; });
/* harmony export (binding) */ __webpack_require__.d(__webpack_exports__, "match5to6", function() { return match5to6; });
/* harmony export (binding) */ __webpack_require__.d(__webpack_exports__, "match1to3", function() { return match1to3; });
/* harmony export (binding) */ __webpack_require__.d(__webpack_exports__, "match1to4", function() { return match1to4; });
/* harmony export (binding) */ __webpack_require__.d(__webpack_exports__, "match1to6", function() { return match1to6; });
/* harmony export (binding) */ __webpack_require__.d(__webpack_exports__, "matchUnsigned", function() { return matchUnsigned; });
/* harmony export (binding) */ __webpack_require__.d(__webpack_exports__, "matchSigned", function() { return matchSigned; });
/* harmony export (binding) */ __webpack_require__.d(__webpack_exports__, "matchOffset", function() { return matchOffset; });
/* harmony export (binding) */ __webpack_require__.d(__webpack_exports__, "matchShortOffset", function() { return matchShortOffset; });
/* harmony export (binding) */ __webpack_require__.d(__webpack_exports__, "matchTimestamp", function() { return matchTimestamp; });
/* harmony export (binding) */ __webpack_require__.d(__webpack_exports__, "matchWord", function() { return matchWord; });
/* harmony export (binding) */ __webpack_require__.d(__webpack_exports__, "addRegexToken", function() { return addRegexToken; });
/* harmony export (binding) */ __webpack_require__.d(__webpack_exports__, "getParseRegexForToken", function() { return getParseRegexForToken; });
/* harmony export (binding) */ __webpack_require__.d(__webpack_exports__, "regexEscape", function() { return regexEscape; });
/* harmony import */ var _utils_type_checks__WEBPACK_IMPORTED_MODULE_0__ = __webpack_require__(/*! ../utils/type-checks */ "./node_modules/ngx-bootstrap/chronos/utils/type-checks.js");

var match1 = /\d/; //       0 - 9
var match2 = /\d\d/; //      00 - 99
var match3 = /\d{3}/; //     000 - 999
var match4 = /\d{4}/; //    0000 - 9999
var match6 = /[+-]?\d{6}/; // -999999 - 999999
var match1to2 = /\d\d?/; //       0 - 99
var match3to4 = /\d\d\d\d?/; //     999 - 9999
var match5to6 = /\d\d\d\d\d\d?/; //   99999 - 999999
var match1to3 = /\d{1,3}/; //       0 - 999
var match1to4 = /\d{1,4}/; //       0 - 9999
var match1to6 = /[+-]?\d{1,6}/; // -999999 - 999999
var matchUnsigned = /\d+/; //       0 - inf
var matchSigned = /[+-]?\d+/; //    -inf - inf
var matchOffset = /Z|[+-]\d\d:?\d\d/gi; // +00:00 -00:00 +0000 -0000 or Z
var matchShortOffset = /Z|[+-]\d\d(?::?\d\d)?/gi; // +00 -00 +00:00 -00:00 +0000 -0000 or Z
var matchTimestamp = /[+-]?\d+(\.\d{1,3})?/; // 123456789 123456789.123
// any word (or two) characters or numbers including two/three word month in arabic.
// includes scottish gaelic two word and hyphenated months
// tslint:disable-next-line
var matchWord = /[0-9]{0,256}['a-z\u00A0-\u05FF\u0700-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF]{1,256}|[\u0600-\u06FF\/]{1,256}(\s*?[\u0600-\u06FF]{1,256}){1,2}/i;
var regexes = {};
function addRegexToken(token, regex, strictRegex) {
    if (Object(_utils_type_checks__WEBPACK_IMPORTED_MODULE_0__["isFunction"])(regex)) {
        regexes[token] = regex;
        return;
    }
    regexes[token] = function (isStrict, locale) {
        return (isStrict && strictRegex) ? strictRegex : regex;
    };
}
function getParseRegexForToken(token, locale) {
    var _strict = false;
    if (!Object(_utils_type_checks__WEBPACK_IMPORTED_MODULE_0__["hasOwnProp"])(regexes, token)) {
        return new RegExp(unescapeFormat(token));
    }
    return regexes[token](_strict, locale);
}
// Code from http://stackoverflow.com/questions/3561493/is-there-a-regexp-escape-function-in-javascript
function unescapeFormat(str) {
    // tslint:disable-next-line
    return regexEscape(str
        .replace('\\', '')
        .replace(/\\(\[)|\\(\])|\[([^\]\[]*)\]|\\(.)/g, function (matched, p1, p2, p3, p4) { return p1 || p2 || p3 || p4; }));
}
function regexEscape(str) {
    return str.replace(/[-\/\\^$*+?.()|[\]{}]/g, '\\$&');
}
//# sourceMappingURL=regex.js.map

/***/ }),

/***/ "./node_modules/ngx-bootstrap/chronos/parse/token.js":
/*!***********************************************************!*\
  !*** ./node_modules/ngx-bootstrap/chronos/parse/token.js ***!
  \***********************************************************/
/*! exports provided: addParseToken, addWeekParseToken, addTimeToArrayFromToken */
/***/ (function(module, __webpack_exports__, __webpack_require__) {

"use strict";
__webpack_require__.r(__webpack_exports__);
/* harmony export (binding) */ __webpack_require__.d(__webpack_exports__, "addParseToken", function() { return addParseToken; });
/* harmony export (binding) */ __webpack_require__.d(__webpack_exports__, "addWeekParseToken", function() { return addWeekParseToken; });
/* harmony export (binding) */ __webpack_require__.d(__webpack_exports__, "addTimeToArrayFromToken", function() { return addTimeToArrayFromToken; });
/* harmony import */ var _utils_type_checks__WEBPACK_IMPORTED_MODULE_0__ = __webpack_require__(/*! ../utils/type-checks */ "./node_modules/ngx-bootstrap/chronos/utils/type-checks.js");

var tokens = {};
function addParseToken(token, callback) {
    var _token = Object(_utils_type_checks__WEBPACK_IMPORTED_MODULE_0__["isString"])(token) ? [token] : token;
    var func = callback;
    if (Object(_utils_type_checks__WEBPACK_IMPORTED_MODULE_0__["isNumber"])(callback)) {
        func = function (input, array, config) {
            array[callback] = Object(_utils_type_checks__WEBPACK_IMPORTED_MODULE_0__["toInt"])(input);
            return config;
        };
    }
    if (Object(_utils_type_checks__WEBPACK_IMPORTED_MODULE_0__["isArray"])(_token) && Object(_utils_type_checks__WEBPACK_IMPORTED_MODULE_0__["isFunction"])(func)) {
        var i = void 0;
        for (i = 0; i < _token.length; i++) {
            tokens[_token[i]] = func;
        }
    }
}
function addWeekParseToken(token, callback) {
    addParseToken(token, function (input, array, config, _token) {
        config._w = config._w || {};
        return callback(input, config._w, config, _token);
    });
}
function addTimeToArrayFromToken(token, input, config) {
    if (input != null && Object(_utils_type_checks__WEBPACK_IMPORTED_MODULE_0__["hasOwnProp"])(tokens, token)) {
        tokens[token](input, config._a, config, token);
    }
    return config;
}
//# sourceMappingURL=token.js.map

/***/ }),

/***/ "./node_modules/ngx-bootstrap/chronos/test/chain.js":
/*!**********************************************************!*\
  !*** ./node_modules/ngx-bootstrap/chronos/test/chain.js ***!
  \**********************************************************/
/*! exports provided: moment, Khronos */
/***/ (function(module, __webpack_exports__, __webpack_require__) {

"use strict";
__webpack_require__.r(__webpack_exports__);
/* harmony export (binding) */ __webpack_require__.d(__webpack_exports__, "moment", function() { return moment; });
/* harmony export (binding) */ __webpack_require__.d(__webpack_exports__, "Khronos", function() { return Khronos; });
/* harmony import */ var _index__WEBPACK_IMPORTED_MODULE_0__ = __webpack_require__(/*! ../index */ "./node_modules/ngx-bootstrap/chronos/index.js");
/* harmony import */ var _utils_date_getters__WEBPACK_IMPORTED_MODULE_1__ = __webpack_require__(/*! ../utils/date-getters */ "./node_modules/ngx-bootstrap/chronos/utils/date-getters.js");
/* harmony import */ var _utils_date_setters__WEBPACK_IMPORTED_MODULE_2__ = __webpack_require__(/*! ../utils/date-setters */ "./node_modules/ngx-bootstrap/chronos/utils/date-setters.js");
/* harmony import */ var _create_clone__WEBPACK_IMPORTED_MODULE_3__ = __webpack_require__(/*! ../create/clone */ "./node_modules/ngx-bootstrap/chronos/create/clone.js");
/* harmony import */ var _utils_type_checks__WEBPACK_IMPORTED_MODULE_4__ = __webpack_require__(/*! ../utils/type-checks */ "./node_modules/ngx-bootstrap/chronos/utils/type-checks.js");
/* harmony import */ var _format__WEBPACK_IMPORTED_MODULE_5__ = __webpack_require__(/*! ../format */ "./node_modules/ngx-bootstrap/chronos/format.js");
/* harmony import */ var _create_from_string_and_format__WEBPACK_IMPORTED_MODULE_6__ = __webpack_require__(/*! ../create/from-string-and-format */ "./node_modules/ngx-bootstrap/chronos/create/from-string-and-format.js");
/* harmony import */ var _units_offset__WEBPACK_IMPORTED_MODULE_7__ = __webpack_require__(/*! ../units/offset */ "./node_modules/ngx-bootstrap/chronos/units/offset.js");
/* harmony import */ var _units_year__WEBPACK_IMPORTED_MODULE_8__ = __webpack_require__(/*! ../units/year */ "./node_modules/ngx-bootstrap/chronos/units/year.js");
/* harmony import */ var _utils_date_compare__WEBPACK_IMPORTED_MODULE_9__ = __webpack_require__(/*! ../utils/date-compare */ "./node_modules/ngx-bootstrap/chronos/utils/date-compare.js");
/* harmony import */ var _units_month__WEBPACK_IMPORTED_MODULE_10__ = __webpack_require__(/*! ../units/month */ "./node_modules/ngx-bootstrap/chronos/units/month.js");
/* harmony import */ var _units_day_of_week__WEBPACK_IMPORTED_MODULE_11__ = __webpack_require__(/*! ../units/day-of-week */ "./node_modules/ngx-bootstrap/chronos/units/day-of-week.js");
/* harmony import */ var _units_week__WEBPACK_IMPORTED_MODULE_12__ = __webpack_require__(/*! ../units/week */ "./node_modules/ngx-bootstrap/chronos/units/week.js");
/* harmony import */ var _units_week_year__WEBPACK_IMPORTED_MODULE_13__ = __webpack_require__(/*! ../units/week-year */ "./node_modules/ngx-bootstrap/chronos/units/week-year.js");
/* harmony import */ var _utils_start_end_of__WEBPACK_IMPORTED_MODULE_14__ = __webpack_require__(/*! ../utils/start-end-of */ "./node_modules/ngx-bootstrap/chronos/utils/start-end-of.js");
/* harmony import */ var _units_quarter__WEBPACK_IMPORTED_MODULE_15__ = __webpack_require__(/*! ../units/quarter */ "./node_modules/ngx-bootstrap/chronos/units/quarter.js");
/* harmony import */ var _units_day_of_year__WEBPACK_IMPORTED_MODULE_16__ = __webpack_require__(/*! ../units/day-of-year */ "./node_modules/ngx-bootstrap/chronos/units/day-of-year.js");
/* harmony import */ var _units_timezone__WEBPACK_IMPORTED_MODULE_17__ = __webpack_require__(/*! ../units/timezone */ "./node_modules/ngx-bootstrap/chronos/units/timezone.js");
/* harmony import */ var _moment_diff__WEBPACK_IMPORTED_MODULE_18__ = __webpack_require__(/*! ../moment/diff */ "./node_modules/ngx-bootstrap/chronos/moment/diff.js");
/* harmony import */ var _moment_calendar__WEBPACK_IMPORTED_MODULE_19__ = __webpack_require__(/*! ../moment/calendar */ "./node_modules/ngx-bootstrap/chronos/moment/calendar.js");
/* harmony import */ var _locale_locales__WEBPACK_IMPORTED_MODULE_20__ = __webpack_require__(/*! ../locale/locales */ "./node_modules/ngx-bootstrap/chronos/locale/locales.js");
/* harmony import */ var _moment_min_max__WEBPACK_IMPORTED_MODULE_21__ = __webpack_require__(/*! ../moment/min-max */ "./node_modules/ngx-bootstrap/chronos/moment/min-max.js");
/* harmony import */ var _duration_constructor__WEBPACK_IMPORTED_MODULE_22__ = __webpack_require__(/*! ../duration/constructor */ "./node_modules/ngx-bootstrap/chronos/duration/constructor.js");
/* harmony import */ var _create_from_anything__WEBPACK_IMPORTED_MODULE_23__ = __webpack_require__(/*! ../create/from-anything */ "./node_modules/ngx-bootstrap/chronos/create/from-anything.js");
/* harmony import */ var _duration_create__WEBPACK_IMPORTED_MODULE_24__ = __webpack_require__(/*! ../duration/create */ "./node_modules/ngx-bootstrap/chronos/duration/create.js");

























var moment = _moment;
function _moment(input, format, localeKey, strict, isUTC) {
    if (input instanceof Khronos) {
        var _date = input.clone();
        return isUTC ? _date.utc() : _date;
    }
    if (Object(_utils_type_checks__WEBPACK_IMPORTED_MODULE_4__["isBoolean"])(localeKey)) {
        return new Khronos(input, format, null, localeKey, isUTC);
    }
    return new Khronos(input, format, localeKey, strict, isUTC);
}
moment.utc = function (input, format, localeKey, strict) {
    return _moment(input, format, localeKey, strict, true);
};
moment.parseZone = function (input, format, localeKey, strict) {
    return _moment(input, format, localeKey, strict, true).parseZone();
};
moment.locale = _locale_locales__WEBPACK_IMPORTED_MODULE_20__["getSetGlobalLocale"];
moment.localeData = function (key) {
    if (key instanceof Khronos) {
        return key.localeData();
    }
    return Object(_locale_locales__WEBPACK_IMPORTED_MODULE_20__["getLocale"])(key);
};
// moment.utc = createUTC;
moment.unix = function (inp) { return new Khronos(inp * 1000); };
moment.ISO_8601 = _create_from_string_and_format__WEBPACK_IMPORTED_MODULE_6__["ISO_8601"];
moment.RFC_2822 = _create_from_string_and_format__WEBPACK_IMPORTED_MODULE_6__["RFC_2822"];
moment.defineLocale = _locale_locales__WEBPACK_IMPORTED_MODULE_20__["defineLocale"];
moment.parseTwoDigitYear = _units_year__WEBPACK_IMPORTED_MODULE_8__["parseTwoDigitYear"];
moment.isDate = _utils_type_checks__WEBPACK_IMPORTED_MODULE_4__["isDate"];
moment.invalid = function _invalid() {
    return new Khronos(new Date(NaN));
};
// duration(inp?: Duration | DateInput | Khronos, unit?: MomentUnitOfTime): Duration;
moment.duration = function (input, unit) {
    var _unit = mapUnitOfTime(unit);
    if (Object(_utils_type_checks__WEBPACK_IMPORTED_MODULE_4__["isDate"])(input)) {
        throw new Error('todo implement');
    }
    if (input == null) {
        return Object(_duration_create__WEBPACK_IMPORTED_MODULE_24__["createDuration"])();
    }
    if (Object(_duration_constructor__WEBPACK_IMPORTED_MODULE_22__["isDuration"])(input)) {
        return Object(_duration_create__WEBPACK_IMPORTED_MODULE_24__["createDuration"])(input, _unit, { _locale: input._locale });
    }
    if (Object(_utils_type_checks__WEBPACK_IMPORTED_MODULE_4__["isString"])(input) || Object(_utils_type_checks__WEBPACK_IMPORTED_MODULE_4__["isNumber"])(input) || Object(_duration_constructor__WEBPACK_IMPORTED_MODULE_22__["isDuration"])(input) || Object(_utils_type_checks__WEBPACK_IMPORTED_MODULE_4__["isObject"])(input)) {
        return Object(_duration_create__WEBPACK_IMPORTED_MODULE_24__["createDuration"])(input, _unit);
    }
    throw new Error('todo implement');
};
moment.min = function _min() {
    var dates = [];
    for (var _i = 0; _i < arguments.length; _i++) {
        dates[_i] = arguments[_i];
    }
    var _firstArg = dates[0];
    var _dates = (Object(_utils_type_checks__WEBPACK_IMPORTED_MODULE_4__["isArray"])(_firstArg) ? _firstArg : dates)
        .map(function (date) { return _moment(date); })
        .map(function (date) { return date.toDate(); });
    var _date = _moment_min_max__WEBPACK_IMPORTED_MODULE_21__["min"].apply(void 0, _dates);
    return new Khronos(_date);
};
moment.max = function _max() {
    var dates = [];
    for (var _i = 0; _i < arguments.length; _i++) {
        dates[_i] = arguments[_i];
    }
    var _firstArg = dates[0];
    var _dates = (Object(_utils_type_checks__WEBPACK_IMPORTED_MODULE_4__["isArray"])(_firstArg) ? _firstArg : dates)
        .map(function (date) { return _moment(date); })
        .map(function (date) { return date.toDate(); });
    var _date = _moment_min_max__WEBPACK_IMPORTED_MODULE_21__["max"].apply(void 0, _dates);
    return new Khronos(_date);
};
moment.locales = function () {
    return Object(_locale_locales__WEBPACK_IMPORTED_MODULE_20__["listLocales"])();
};
var _unitsPriority = {
    year: 1,
    month: 8,
    week: 5,
    isoWeek: 5,
    day: 11,
    weekday: 11,
    isoWeekday: 11,
    hours: 13,
    weekYear: 1,
    isoWeekYear: 1,
    quarter: 7,
    date: 9,
    dayOfYear: 4,
    minutes: 14,
    seconds: 15,
    milliseconds: 16
};
// todo: do I need 2 mappers?
var _timeHashMap = {
    y: 'year',
    years: 'year',
    year: 'year',
    M: 'month',
    months: 'month',
    month: 'month',
    w: 'week',
    weeks: 'week',
    week: 'week',
    d: 'day',
    days: 'day',
    day: 'day',
    date: 'date',
    dates: 'date',
    D: 'date',
    h: 'hours',
    hour: 'hours',
    hours: 'hours',
    m: 'minutes',
    minute: 'minutes',
    minutes: 'minutes',
    s: 'seconds',
    second: 'seconds',
    seconds: 'seconds',
    ms: 'milliseconds',
    millisecond: 'milliseconds',
    milliseconds: 'milliseconds',
    quarter: 'quarter',
    quarters: 'quarter',
    q: 'quarter',
    Q: 'quarter',
    isoWeek: 'isoWeek',
    isoWeeks: 'isoWeek',
    W: 'isoWeek',
    weekYear: 'weekYear',
    weekYears: 'weekYear',
    gg: 'weekYears',
    isoWeekYear: 'isoWeekYear',
    isoWeekYears: 'isoWeekYear',
    GG: 'isoWeekYear',
    dayOfYear: 'dayOfYear',
    dayOfYears: 'dayOfYear',
    DDD: 'dayOfYear',
    weekday: 'weekday',
    weekdays: 'weekday',
    e: 'weekday',
    isoWeekday: 'isoWeekday',
    isoWeekdays: 'isoWeekday',
    E: 'isoWeekday'
};
function mapUnitOfTime(period) {
    return _timeHashMap[period];
}
function mapMomentInputObject(obj) {
    var _res = {};
    return Object.keys(obj)
        .reduce(function (res, key) {
        res[mapUnitOfTime(key)] = obj[key];
        return res;
    }, _res);
}
var Khronos = /** @class */ (function () {
    function Khronos(input, format, localeKey, strict, isUTC, offset) {
        if (strict === void 0) { strict = false; }
        if (isUTC === void 0) { isUTC = false; }
        this._date = new Date();
        this._isUTC = false;
        // locale will be needed to format invalid date message
        this._locale = Object(_locale_locales__WEBPACK_IMPORTED_MODULE_20__["getLocale"])(localeKey);
        // parse invalid input
        if (input === '' || input === null || (Object(_utils_type_checks__WEBPACK_IMPORTED_MODULE_4__["isNumber"])(input) && isNaN(input))) {
            this._date = new Date(NaN);
            return this;
        }
        this._isUTC = isUTC;
        if (this._isUTC) {
            this._offset = 0;
        }
        if (offset || offset === 0) {
            this._offset = offset;
        }
        this._isStrict = strict;
        this._format = format;
        if (!input && input !== 0 && !format) {
            this._date = new Date();
            return this;
        }
        if (Object(_utils_type_checks__WEBPACK_IMPORTED_MODULE_4__["isDate"])(input)) {
            this._date = Object(_create_clone__WEBPACK_IMPORTED_MODULE_3__["cloneDate"])(input);
            return this;
        }
        // this._date = parseDate(input, format, localeKey, strict, isUTC);
        var config = Object(_create_from_anything__WEBPACK_IMPORTED_MODULE_23__["createLocalOrUTC"])(input, format, localeKey, strict, isUTC);
        this._date = config._d;
        this._offset = Object(_utils_type_checks__WEBPACK_IMPORTED_MODULE_4__["isNumber"])(config._offset) ? config._offset : this._offset;
        this._isUTC = config._isUTC;
        this._isStrict = config._strict;
        this._format = config._f;
        this._tzm = config._tzm;
    }
    Khronos.prototype._toConfig = function () {
        return { _isUTC: this._isUTC, _locale: this._locale, _offset: this._offset, _tzm: this._tzm };
    };
    Khronos.prototype.locale = function (localeKey) {
        if (Object(_utils_type_checks__WEBPACK_IMPORTED_MODULE_4__["isUndefined"])(localeKey)) {
            return this._locale._abbr;
        }
        if (localeKey instanceof Khronos) {
            this._locale = localeKey._locale;
            return this;
        }
        var newLocaleData = Object(_locale_locales__WEBPACK_IMPORTED_MODULE_20__["getLocale"])(localeKey);
        if (newLocaleData != null) {
            this._locale = newLocaleData;
        }
        return this;
    };
    Khronos.prototype.localeData = function () {
        return this._locale;
    };
    // Basic
    // Basic
    Khronos.prototype.add = 
    // Basic
    function (val, period) {
        var _this = this;
        if (Object(_utils_type_checks__WEBPACK_IMPORTED_MODULE_4__["isString"])(val)) {
            this._date = Object(_index__WEBPACK_IMPORTED_MODULE_0__["add"])(this._date, parseInt(val, 10), mapUnitOfTime(period));
        }
        if (Object(_utils_type_checks__WEBPACK_IMPORTED_MODULE_4__["isNumber"])(val)) {
            this._date = Object(_index__WEBPACK_IMPORTED_MODULE_0__["add"])(this._date, val, mapUnitOfTime(period));
        }
        if (Object(_utils_type_checks__WEBPACK_IMPORTED_MODULE_4__["isObject"])(val)) {
            var _mapped_1 = mapMomentInputObject(val);
            Object.keys(_mapped_1)
                .forEach(function (key) { return Object(_index__WEBPACK_IMPORTED_MODULE_0__["add"])(_this._date, _mapped_1[key], key); });
        }
        return this;
    };
    // fixme: for some reason here 'null' for time is fine
    // fixme: for some reason here 'null' for time is fine
    Khronos.prototype.calendar = 
    // fixme: for some reason here 'null' for time is fine
    function (time, formats) {
        var _time = time instanceof Khronos ? time : new Khronos(time || new Date());
        var _offset = (this._offset || 0) - (_time._offset || 0);
        var _config = Object.assign(this._toConfig(), { _offset: _offset });
        return Object(_moment_calendar__WEBPACK_IMPORTED_MODULE_19__["calendar"])(this._date, _time._date, formats, this._locale, _config);
    };
    Khronos.prototype.clone = function () {
        var localeKey = this._locale && this._locale._abbr || 'en';
        // return new Khronos(cloneDate(this._date), this._format, localeKey, this._isStrict, this._isUTC);
        // fails if isUTC and offset
        // return new Khronos(new Date(this.valueOf()),
        return new Khronos(this._date, this._format, localeKey, this._isStrict, this._isUTC, this._offset);
    };
    Khronos.prototype.diff = function (b, unitOfTime, precise) {
        var unit = mapUnitOfTime(unitOfTime);
        var _b = b instanceof Khronos ? b : new Khronos(b);
        // const zoneDelta = (_b.utcOffset() - this.utcOffset());
        // const config = Object.assign(this._toConfig(), {
        //   _offset: 0,
        //   _isUTC: true,
        //   _zoneDelta: zoneDelta
        // });
        // return diff(new Date(this.valueOf()), new Date(_b.valueOf()), unit, precise, config);
        return Object(_moment_diff__WEBPACK_IMPORTED_MODULE_18__["diff"])(this._date, _b.toDate(), unit, precise, this._toConfig());
    };
    Khronos.prototype.endOf = function (period) {
        var _per = mapUnitOfTime(period);
        this._date = Object(_utils_start_end_of__WEBPACK_IMPORTED_MODULE_14__["endOf"])(this._date, _per, this._isUTC);
        return this;
    };
    Khronos.prototype.format = function (format) {
        return Object(_format__WEBPACK_IMPORTED_MODULE_5__["formatDate"])(this._date, format, this._locale && this._locale._abbr, this._isUTC, this._offset);
    };
    // todo: implement
    // todo: implement
    Khronos.prototype.from = 
    // todo: implement
    function (time, withoutSuffix) {
        var _time = _moment(time);
        if (this.isValid() && _time.isValid()) {
            return Object(_duration_create__WEBPACK_IMPORTED_MODULE_24__["createDuration"])({ to: this.toDate(), from: _time.toDate() })
                .locale(this.locale())
                .humanize(!withoutSuffix);
        }
        return this.localeData().invalidDate;
    };
    Khronos.prototype.fromNow = function (withoutSuffix) {
        return this.from(new Date(), withoutSuffix);
    };
    Khronos.prototype.to = function (inp, suffix) {
        throw new Error("TODO: Implement");
    };
    Khronos.prototype.toNow = function (withoutPrefix) {
        throw new Error("TODO: Implement");
    };
    Khronos.prototype.subtract = function (val, period) {
        var _this = this;
        if (Object(_utils_type_checks__WEBPACK_IMPORTED_MODULE_4__["isString"])(val)) {
            this._date = Object(_index__WEBPACK_IMPORTED_MODULE_0__["subtract"])(this._date, parseInt(val, 10), mapUnitOfTime(period));
            return this;
        }
        if (Object(_utils_type_checks__WEBPACK_IMPORTED_MODULE_4__["isNumber"])(val)) {
            this._date = Object(_index__WEBPACK_IMPORTED_MODULE_0__["subtract"])(this._date, val, mapUnitOfTime(period));
        }
        if (Object(_utils_type_checks__WEBPACK_IMPORTED_MODULE_4__["isObject"])(val)) {
            var _mapped_2 = mapMomentInputObject(val);
            Object.keys(_mapped_2)
                .forEach(function (key) { return Object(_index__WEBPACK_IMPORTED_MODULE_0__["subtract"])(_this._date, _mapped_2[key], key); });
        }
        return this;
    };
    Khronos.prototype.get = function (period) {
        if (period === 'dayOfYear') {
            return this.dayOfYear();
        }
        var unit = mapUnitOfTime(period);
        switch (unit) {
            case 'year':
                return this.year();
            case 'month':
                return this.month();
            // | 'week'
            case 'date':
                return this.date();
            case 'day':
                return this.day();
            case 'hours':
                return this.hours();
            case 'minutes':
                return this.minutes();
            case 'seconds':
                return this.seconds();
            case 'milliseconds':
                return this.milliseconds();
            case 'week':
                return this.week();
            case 'isoWeek':
                return this.isoWeek();
            case 'weekYear':
                return this.weekYear();
            case 'isoWeekYear':
                return this.isoWeekYear();
            case 'weekday':
                return this.weekday();
            case 'isoWeekday':
                return this.isoWeekday();
            case 'quarter':
                return this.quarter();
            default:
                throw new Error("Unknown moment.get('" + period + "')");
        }
    };
    Khronos.prototype.set = function (period, input) {
        var _this = this;
        if (Object(_utils_type_checks__WEBPACK_IMPORTED_MODULE_4__["isString"])(period)) {
            var unit = mapUnitOfTime(period);
            switch (unit) {
                case 'year':
                    return this.year(input);
                case 'month':
                    return this.month(input);
                // | 'week'
                case 'day':
                    return this.day(input);
                case 'date':
                    return this.date(input);
                case 'hours':
                    return this.hours(input);
                case 'minutes':
                    return this.minutes(input);
                case 'seconds':
                    return this.seconds(input);
                case 'milliseconds':
                    return this.milliseconds(input);
                case 'week':
                    return this.week(input);
                case 'isoWeek':
                    return this.isoWeek(input);
                case 'weekYear':
                    return this.weekYear(input);
                case 'isoWeekYear':
                    return this.isoWeekYear(input);
                case 'weekday':
                    return this.weekday(input);
                case 'isoWeekday':
                    return this.isoWeekday(input);
                case 'quarter':
                    return this.quarter(input);
                default:
                    throw new Error("Unknown moment.get('" + period + "')");
            }
        }
        if (Object(_utils_type_checks__WEBPACK_IMPORTED_MODULE_4__["isObject"])(period)) {
            var _mapped_3 = mapMomentInputObject(period);
            Object.keys(_mapped_3)
                .sort(function (a, b) {
                return _unitsPriority[a] - _unitsPriority[b];
            })
                .forEach(function (key) { return _this.set(key, _mapped_3[key]); });
        }
        return this;
    };
    Khronos.prototype.toString = function () {
        return this.format('ddd MMM DD YYYY HH:mm:ss [GMT]ZZ');
    };
    Khronos.prototype.toISOString = function () {
        if (!this.isValid()) {
            return null;
        }
        if (Object(_utils_date_getters__WEBPACK_IMPORTED_MODULE_1__["getFullYear"])(this._date, true) < 0 || Object(_utils_date_getters__WEBPACK_IMPORTED_MODULE_1__["getFullYear"])(this._date, true) > 9999) {
            return this.format('YYYYYY-MM-DD[T]HH:mm:ss.SSS[Z]');
        }
        if (Object(_utils_type_checks__WEBPACK_IMPORTED_MODULE_4__["isFunction"])(Date.prototype.toISOString)) {
            // native implementation is ~50x faster, use it when we can
            return this.toDate().toISOString();
        }
        return this.format('YYYY-MM-DD[T]HH:mm:ss.SSS[Z]');
    };
    Khronos.prototype.inspect = function () {
        throw new Error('TODO: implement');
    };
    Khronos.prototype.toJSON = function () {
        return this.toISOString();
    };
    Khronos.prototype.toDate = function () {
        return new Date(this.valueOf());
    };
    Khronos.prototype.toObject = function () {
        return {
            // years: getFullYear(this._date, this._isUTC),
            // months: getMonth(this._date, this._isUTC),
            year: Object(_utils_date_getters__WEBPACK_IMPORTED_MODULE_1__["getFullYear"])(this._date, this._isUTC),
            month: Object(_utils_date_getters__WEBPACK_IMPORTED_MODULE_1__["getMonth"])(this._date, this._isUTC),
            date: Object(_utils_date_getters__WEBPACK_IMPORTED_MODULE_1__["getDate"])(this._date, this._isUTC),
            hours: Object(_utils_date_getters__WEBPACK_IMPORTED_MODULE_1__["getHours"])(this._date, this._isUTC),
            minutes: Object(_utils_date_getters__WEBPACK_IMPORTED_MODULE_1__["getMinutes"])(this._date, this._isUTC),
            seconds: Object(_utils_date_getters__WEBPACK_IMPORTED_MODULE_1__["getSeconds"])(this._date, this._isUTC),
            milliseconds: Object(_utils_date_getters__WEBPACK_IMPORTED_MODULE_1__["getMilliseconds"])(this._date, this._isUTC)
        };
    };
    Khronos.prototype.toArray = function () {
        return [this.year(), this.month(), this.date(), this.hour(), this.minute(), this.second(), this.millisecond()];
    };
    // Dates boolean algebra
    // Dates boolean algebra
    Khronos.prototype.isAfter = 
    // Dates boolean algebra
    function (date, unit) {
        var _unit = unit ? mapUnitOfTime(unit) : void 0;
        return Object(_utils_date_compare__WEBPACK_IMPORTED_MODULE_9__["isAfter"])(this._date, date.toDate(), _unit);
    };
    Khronos.prototype.isBefore = function (date, unit) {
        var _unit = unit ? mapUnitOfTime(unit) : void 0;
        return Object(_utils_date_compare__WEBPACK_IMPORTED_MODULE_9__["isBefore"])(this.toDate(), date.toDate(), _unit);
    };
    Khronos.prototype.isBetween = function (from, to, unit, inclusivity) {
        var _unit = unit ? mapUnitOfTime(unit) : void 0;
        return Object(_utils_date_compare__WEBPACK_IMPORTED_MODULE_9__["isBetween"])(this.toDate(), from.toDate(), to.toDate(), _unit, inclusivity);
    };
    Khronos.prototype.isSame = function (date, unit) {
        var _unit = unit ? mapUnitOfTime(unit) : void 0;
        return Object(_utils_date_compare__WEBPACK_IMPORTED_MODULE_9__["isSame"])(this._date, date.toDate(), _unit);
    };
    Khronos.prototype.isSameOrAfter = function (date, unit) {
        var _unit = unit ? mapUnitOfTime(unit) : void 0;
        return Object(_utils_date_compare__WEBPACK_IMPORTED_MODULE_9__["isSameOrAfter"])(this._date, date.toDate(), _unit);
    };
    Khronos.prototype.isSameOrBefore = function (date, unit) {
        var _unit = unit ? mapUnitOfTime(unit) : void 0;
        return Object(_utils_date_compare__WEBPACK_IMPORTED_MODULE_9__["isSameOrBefore"])(this._date, date.toDate(), _unit);
    };
    Khronos.prototype.isValid = function () {
        return Object(_utils_type_checks__WEBPACK_IMPORTED_MODULE_4__["isDateValid"])(this._date);
    };
    Khronos.prototype.valueOf = function () {
        return this._date.valueOf() - ((this._offset || 0) * 60000);
    };
    Khronos.prototype.unix = function () {
        // return getUnixTime(this._date);
        return Math.floor(this.valueOf() / 1000);
    };
    Khronos.prototype.utcOffset = function (b, keepLocalTime) {
        var _config = this._toConfig();
        if (!b && b !== 0) {
            return Object(_units_offset__WEBPACK_IMPORTED_MODULE_7__["getUTCOffset"])(this._date, _config);
        }
        this._date = Object(_units_offset__WEBPACK_IMPORTED_MODULE_7__["setUTCOffset"])(this._date, b, keepLocalTime, false, _config);
        this._offset = _config._offset;
        this._isUTC = _config._isUTC;
        return this;
    };
    Khronos.prototype.utc = function (keepLocalTime) {
        return this.utcOffset(0, keepLocalTime);
    };
    Khronos.prototype.local = function (keepLocalTime) {
        if (this._isUTC) {
            this.utcOffset(0, keepLocalTime);
            this._isUTC = false;
            if (keepLocalTime) {
                this.subtract(Object(_units_offset__WEBPACK_IMPORTED_MODULE_7__["getDateOffset"])(this._date), 'm');
            }
        }
        return this;
    };
    Khronos.prototype.parseZone = function (input) {
        var _config = this._toConfig();
        this._date = Object(_units_offset__WEBPACK_IMPORTED_MODULE_7__["setOffsetToParsedOffset"])(this._date, input, _config);
        this._offset = _config._offset;
        this._isUTC = _config._isUTC;
        return this;
    };
    Khronos.prototype.hasAlignedHourOffset = function (input) {
        return Object(_units_offset__WEBPACK_IMPORTED_MODULE_7__["hasAlignedHourOffset"])(this._date, input ? input._date : void 0);
    };
    Khronos.prototype.isDST = function () {
        return Object(_units_offset__WEBPACK_IMPORTED_MODULE_7__["isDaylightSavingTime"])(this._date);
    };
    Khronos.prototype.isLocal = function () {
        return !this._isUTC;
    };
    Khronos.prototype.isUtcOffset = function () {
        return this._isUTC;
    };
    Khronos.prototype.isUTC = function () {
        return this.isUtc();
    };
    Khronos.prototype.isUtc = function () {
        return this._isUTC && this._offset === 0;
    };
    // Timezone
    // Timezone
    Khronos.prototype.zoneAbbr = 
    // Timezone
    function () {
        return Object(_units_timezone__WEBPACK_IMPORTED_MODULE_17__["getZoneAbbr"])(this._isUTC);
    };
    Khronos.prototype.zoneName = function () {
        return Object(_units_timezone__WEBPACK_IMPORTED_MODULE_17__["getZoneName"])(this._isUTC);
    };
    Khronos.prototype.year = function (year) {
        if (!year && year !== 0) {
            return Object(_utils_date_getters__WEBPACK_IMPORTED_MODULE_1__["getFullYear"])(this._date, this._isUTC);
        }
        this._date = Object(_create_clone__WEBPACK_IMPORTED_MODULE_3__["cloneDate"])(Object(_utils_date_setters__WEBPACK_IMPORTED_MODULE_2__["setFullYear"])(this._date, year));
        return this;
    };
    Khronos.prototype.weekYear = function (val) {
        if (!val && val !== 0) {
            return Object(_units_week_year__WEBPACK_IMPORTED_MODULE_13__["getWeekYear"])(this._date, this._locale, this.isUTC());
        }
        var date = Object(_units_week_year__WEBPACK_IMPORTED_MODULE_13__["getSetWeekYear"])(this._date, val, this._locale, this.isUTC());
        if (Object(_utils_type_checks__WEBPACK_IMPORTED_MODULE_4__["isDate"])(date)) {
            this._date = date;
        }
        return this;
    };
    Khronos.prototype.isoWeekYear = function (val) {
        if (!val && val !== 0) {
            return Object(_units_week_year__WEBPACK_IMPORTED_MODULE_13__["getISOWeekYear"])(this._date, this.isUTC());
        }
        var date = Object(_units_week_year__WEBPACK_IMPORTED_MODULE_13__["getSetISOWeekYear"])(this._date, val, this.isUtc());
        if (Object(_utils_type_checks__WEBPACK_IMPORTED_MODULE_4__["isDate"])(date)) {
            this._date = date;
        }
        return this;
    };
    Khronos.prototype.isLeapYear = function () {
        return Object(_units_year__WEBPACK_IMPORTED_MODULE_8__["isLeapYear"])(Object(_utils_date_getters__WEBPACK_IMPORTED_MODULE_1__["getFullYear"])(this.toDate(), this.isUTC()));
    };
    Khronos.prototype.month = function (month) {
        if (!month && month !== 0) {
            return Object(_utils_date_getters__WEBPACK_IMPORTED_MODULE_1__["getMonth"])(this._date, this._isUTC);
        }
        var _month = month;
        if (Object(_utils_type_checks__WEBPACK_IMPORTED_MODULE_4__["isString"])(month)) {
            var locale = this._locale || Object(_locale_locales__WEBPACK_IMPORTED_MODULE_20__["getLocale"])();
            _month = locale.monthsParse(month);
        }
        if (Object(_utils_type_checks__WEBPACK_IMPORTED_MODULE_4__["isNumber"])(_month)) {
            this._date = Object(_create_clone__WEBPACK_IMPORTED_MODULE_3__["cloneDate"])(Object(_utils_date_setters__WEBPACK_IMPORTED_MODULE_2__["setMonth"])(this._date, _month, this._isUTC));
        }
        return this;
    };
    Khronos.prototype.hour = function (hours) {
        return this.hours(hours);
    };
    Khronos.prototype.hours = function (hours) {
        if (!hours && hours !== 0) {
            return Object(_utils_date_getters__WEBPACK_IMPORTED_MODULE_1__["getHours"])(this._date, this._isUTC);
        }
        this._date = Object(_create_clone__WEBPACK_IMPORTED_MODULE_3__["cloneDate"])(Object(_utils_date_setters__WEBPACK_IMPORTED_MODULE_2__["setHours"])(this._date, hours, this._isUTC));
        return this;
    };
    Khronos.prototype.minute = function (minutes) {
        return this.minutes(minutes);
    };
    Khronos.prototype.minutes = function (minutes) {
        if (!minutes && minutes !== 0) {
            return Object(_utils_date_getters__WEBPACK_IMPORTED_MODULE_1__["getMinutes"])(this._date, this._isUTC);
        }
        this._date = Object(_create_clone__WEBPACK_IMPORTED_MODULE_3__["cloneDate"])(Object(_utils_date_setters__WEBPACK_IMPORTED_MODULE_2__["setMinutes"])(this._date, minutes, this._isUTC));
        return this;
    };
    Khronos.prototype.second = function (seconds) {
        return this.seconds(seconds);
    };
    Khronos.prototype.seconds = function (seconds) {
        if (!seconds && seconds !== 0) {
            return Object(_utils_date_getters__WEBPACK_IMPORTED_MODULE_1__["getSeconds"])(this._date, this._isUTC);
        }
        this._date = Object(_create_clone__WEBPACK_IMPORTED_MODULE_3__["cloneDate"])(Object(_utils_date_setters__WEBPACK_IMPORTED_MODULE_2__["setSeconds"])(this._date, seconds, this._isUTC));
        return this;
    };
    Khronos.prototype.millisecond = function (ms) {
        return this.milliseconds(ms);
    };
    Khronos.prototype.milliseconds = function (seconds) {
        if (!seconds && seconds !== 0) {
            return Object(_utils_date_getters__WEBPACK_IMPORTED_MODULE_1__["getMilliseconds"])(this._date, this._isUTC);
        }
        this._date = Object(_create_clone__WEBPACK_IMPORTED_MODULE_3__["cloneDate"])(Object(_utils_date_setters__WEBPACK_IMPORTED_MODULE_2__["setMilliseconds"])(this._date, seconds, this._isUTC));
        return this;
    };
    Khronos.prototype.date = function (date) {
        if (!date && date !== 0) {
            return Object(_utils_date_getters__WEBPACK_IMPORTED_MODULE_1__["getDate"])(this._date, this._isUTC);
        }
        this._date = Object(_create_clone__WEBPACK_IMPORTED_MODULE_3__["cloneDate"])(Object(_utils_date_setters__WEBPACK_IMPORTED_MODULE_2__["setDate"])(this._date, date, this._isUTC));
        return this;
    };
    Khronos.prototype.day = function (input) {
        if (!input && input !== 0) {
            return Object(_units_day_of_week__WEBPACK_IMPORTED_MODULE_11__["getDayOfWeek"])(this._date, this._isUTC);
        }
        var _input = input;
        if (Object(_utils_type_checks__WEBPACK_IMPORTED_MODULE_4__["isString"])(input)) {
            _input = Object(_units_day_of_week__WEBPACK_IMPORTED_MODULE_11__["parseWeekday"])(input, this._locale);
        }
        if (Object(_utils_type_checks__WEBPACK_IMPORTED_MODULE_4__["isNumber"])(_input)) {
            this._date = Object(_units_day_of_week__WEBPACK_IMPORTED_MODULE_11__["setDayOfWeek"])(this._date, _input, this._locale, this._isUTC);
        }
        return this;
    };
    Khronos.prototype.weekday = function (val) {
        if (!val && val !== 0) {
            return Object(_units_day_of_week__WEBPACK_IMPORTED_MODULE_11__["getLocaleDayOfWeek"])(this._date, this._locale, this._isUTC);
        }
        this._date = Object(_units_day_of_week__WEBPACK_IMPORTED_MODULE_11__["setLocaleDayOfWeek"])(this._date, val, { locale: this._locale, isUTC: this._isUTC });
        return this;
    };
    Khronos.prototype.isoWeekday = function (val) {
        if (!val && val !== 0) {
            return Object(_units_day_of_week__WEBPACK_IMPORTED_MODULE_11__["getISODayOfWeek"])(this._date);
        }
        this._date = Object(_units_day_of_week__WEBPACK_IMPORTED_MODULE_11__["setISODayOfWeek"])(this._date, val);
        return this;
    };
    Khronos.prototype.dayOfYear = function (val) {
        if (!val && val !== 0) {
            return Object(_units_day_of_year__WEBPACK_IMPORTED_MODULE_16__["getDayOfYear"])(this._date);
        }
        this._date = Object(_units_day_of_year__WEBPACK_IMPORTED_MODULE_16__["setDayOfYear"])(this._date, val);
        return this;
    };
    Khronos.prototype.week = function (input) {
        if (!input && input !== 0) {
            return Object(_units_week__WEBPACK_IMPORTED_MODULE_12__["getWeek"])(this._date, this._locale);
        }
        this._date = Object(_units_week__WEBPACK_IMPORTED_MODULE_12__["setWeek"])(this._date, input, this._locale);
        return this;
    };
    Khronos.prototype.weeks = function (input) {
        return this.week(input);
    };
    Khronos.prototype.isoWeek = function (val) {
        if (!val && val !== 0) {
            return Object(_units_week__WEBPACK_IMPORTED_MODULE_12__["getISOWeek"])(this._date);
        }
        this._date = Object(_units_week__WEBPACK_IMPORTED_MODULE_12__["setISOWeek"])(this._date, val);
        return this;
    };
    Khronos.prototype.isoWeeks = function (val) {
        return this.isoWeek(val);
    };
    Khronos.prototype.weeksInYear = function () {
        return Object(_units_week_year__WEBPACK_IMPORTED_MODULE_13__["getWeeksInYear"])(this._date, this._isUTC, this._locale);
    };
    Khronos.prototype.isoWeeksInYear = function () {
        return Object(_units_week_year__WEBPACK_IMPORTED_MODULE_13__["getISOWeeksInYear"])(this._date, this._isUTC);
    };
    Khronos.prototype.daysInMonth = function () {
        return Object(_units_month__WEBPACK_IMPORTED_MODULE_10__["daysInMonth"])(Object(_utils_date_getters__WEBPACK_IMPORTED_MODULE_1__["getFullYear"])(this._date, this._isUTC), Object(_utils_date_getters__WEBPACK_IMPORTED_MODULE_1__["getMonth"])(this._date, this._isUTC));
    };
    Khronos.prototype.quarter = function (val) {
        if (!val && val !== 0) {
            return Object(_units_quarter__WEBPACK_IMPORTED_MODULE_15__["getQuarter"])(this._date, this._isUTC);
        }
        this._date = Object(_units_quarter__WEBPACK_IMPORTED_MODULE_15__["setQuarter"])(this._date, val, this._isUTC);
        return this;
    };
    Khronos.prototype.quarters = function (val) {
        return this.quarter(val);
    };
    Khronos.prototype.startOf = function (period) {
        var _per = mapUnitOfTime(period);
        this._date = Object(_utils_start_end_of__WEBPACK_IMPORTED_MODULE_14__["startOf"])(this._date, _per, this._isUTC);
        return this;
    };
    return Khronos;
}());

//# sourceMappingURL=chain.js.map

/***/ }),

/***/ "./node_modules/ngx-bootstrap/chronos/units/aliases.js":
/*!*************************************************************!*\
  !*** ./node_modules/ngx-bootstrap/chronos/units/aliases.js ***!
  \*************************************************************/
/*! exports provided: addUnitAlias, normalizeUnits, normalizeObjectUnits */
/***/ (function(module, __webpack_exports__, __webpack_require__) {

"use strict";
__webpack_require__.r(__webpack_exports__);
/* harmony export (binding) */ __webpack_require__.d(__webpack_exports__, "addUnitAlias", function() { return addUnitAlias; });
/* harmony export (binding) */ __webpack_require__.d(__webpack_exports__, "normalizeUnits", function() { return normalizeUnits; });
/* harmony export (binding) */ __webpack_require__.d(__webpack_exports__, "normalizeObjectUnits", function() { return normalizeObjectUnits; });
/* harmony import */ var _utils_type_checks__WEBPACK_IMPORTED_MODULE_0__ = __webpack_require__(/*! ../utils/type-checks */ "./node_modules/ngx-bootstrap/chronos/utils/type-checks.js");

var aliases = {};
var _mapUnits = {
    date: 'day',
    hour: 'hours',
    minute: 'minutes',
    second: 'seconds',
    millisecond: 'milliseconds'
};
function addUnitAlias(unit, shorthand) {
    var lowerCase = unit.toLowerCase();
    var _unit = unit;
    if (lowerCase in _mapUnits) {
        _unit = _mapUnits[lowerCase];
    }
    aliases[lowerCase] = aliases[lowerCase + "s"] = aliases[shorthand] = _unit;
}
function normalizeUnits(units) {
    return Object(_utils_type_checks__WEBPACK_IMPORTED_MODULE_0__["isString"])(units) ? aliases[units] || aliases[units.toLowerCase()] : undefined;
}
function normalizeObjectUnits(inputObject) {
    var normalizedInput = {};
    var normalizedProp;
    var prop;
    for (prop in inputObject) {
        if (Object(_utils_type_checks__WEBPACK_IMPORTED_MODULE_0__["hasOwnProp"])(inputObject, prop)) {
            normalizedProp = normalizeUnits(prop);
            if (normalizedProp) {
                normalizedInput[normalizedProp] = inputObject[prop];
            }
        }
    }
    return normalizedInput;
}
//# sourceMappingURL=aliases.js.map

/***/ }),

/***/ "./node_modules/ngx-bootstrap/chronos/units/constants.js":
/*!***************************************************************!*\
  !*** ./node_modules/ngx-bootstrap/chronos/units/constants.js ***!
  \***************************************************************/
/*! exports provided: YEAR, MONTH, DATE, HOUR, MINUTE, SECOND, MILLISECOND, WEEK, WEEKDAY */
/***/ (function(module, __webpack_exports__, __webpack_require__) {

"use strict";
__webpack_require__.r(__webpack_exports__);
/* harmony export (binding) */ __webpack_require__.d(__webpack_exports__, "YEAR", function() { return YEAR; });
/* harmony export (binding) */ __webpack_require__.d(__webpack_exports__, "MONTH", function() { return MONTH; });
/* harmony export (binding) */ __webpack_require__.d(__webpack_exports__, "DATE", function() { return DATE; });
/* harmony export (binding) */ __webpack_require__.d(__webpack_exports__, "HOUR", function() { return HOUR; });
/* harmony export (binding) */ __webpack_require__.d(__webpack_exports__, "MINUTE", function() { return MINUTE; });
/* harmony export (binding) */ __webpack_require__.d(__webpack_exports__, "SECOND", function() { return SECOND; });
/* harmony export (binding) */ __webpack_require__.d(__webpack_exports__, "MILLISECOND", function() { return MILLISECOND; });
/* harmony export (binding) */ __webpack_require__.d(__webpack_exports__, "WEEK", function() { return WEEK; });
/* harmony export (binding) */ __webpack_require__.d(__webpack_exports__, "WEEKDAY", function() { return WEEKDAY; });
// place in new Date([array])
var YEAR = 0;
var MONTH = 1;
var DATE = 2;
var HOUR = 3;
var MINUTE = 4;
var SECOND = 5;
var MILLISECOND = 6;
var WEEK = 7;
var WEEKDAY = 8;
//# sourceMappingURL=constants.js.map

/***/ }),

/***/ "./node_modules/ngx-bootstrap/chronos/units/day-of-month.js":
/*!******************************************************************!*\
  !*** ./node_modules/ngx-bootstrap/chronos/units/day-of-month.js ***!
  \******************************************************************/
/*! no exports provided */
/***/ (function(module, __webpack_exports__, __webpack_require__) {

"use strict";
__webpack_require__.r(__webpack_exports__);
/* harmony import */ var _format_format__WEBPACK_IMPORTED_MODULE_0__ = __webpack_require__(/*! ../format/format */ "./node_modules/ngx-bootstrap/chronos/format/format.js");
/* harmony import */ var _utils_date_getters__WEBPACK_IMPORTED_MODULE_1__ = __webpack_require__(/*! ../utils/date-getters */ "./node_modules/ngx-bootstrap/chronos/utils/date-getters.js");
/* harmony import */ var _parse_regex__WEBPACK_IMPORTED_MODULE_2__ = __webpack_require__(/*! ../parse/regex */ "./node_modules/ngx-bootstrap/chronos/parse/regex.js");
/* harmony import */ var _parse_token__WEBPACK_IMPORTED_MODULE_3__ = __webpack_require__(/*! ../parse/token */ "./node_modules/ngx-bootstrap/chronos/parse/token.js");
/* harmony import */ var _constants__WEBPACK_IMPORTED_MODULE_4__ = __webpack_require__(/*! ./constants */ "./node_modules/ngx-bootstrap/chronos/units/constants.js");
/* harmony import */ var _utils_type_checks__WEBPACK_IMPORTED_MODULE_5__ = __webpack_require__(/*! ../utils/type-checks */ "./node_modules/ngx-bootstrap/chronos/utils/type-checks.js");
/* harmony import */ var _aliases__WEBPACK_IMPORTED_MODULE_6__ = __webpack_require__(/*! ./aliases */ "./node_modules/ngx-bootstrap/chronos/units/aliases.js");
/* harmony import */ var _priorities__WEBPACK_IMPORTED_MODULE_7__ = __webpack_require__(/*! ./priorities */ "./node_modules/ngx-bootstrap/chronos/units/priorities.js");








// FORMATTING
Object(_format_format__WEBPACK_IMPORTED_MODULE_0__["addFormatToken"])('D', ['DD', 2, false], 'Do', function (date, opts) {
    return Object(_utils_date_getters__WEBPACK_IMPORTED_MODULE_1__["getDate"])(date, opts.isUTC).toString(10);
});
// ALIASES
Object(_aliases__WEBPACK_IMPORTED_MODULE_6__["addUnitAlias"])('date', 'D');
// PRIOROITY
Object(_priorities__WEBPACK_IMPORTED_MODULE_7__["addUnitPriority"])('date', 9);
// PARSING
Object(_parse_regex__WEBPACK_IMPORTED_MODULE_2__["addRegexToken"])('D', _parse_regex__WEBPACK_IMPORTED_MODULE_2__["match1to2"]);
Object(_parse_regex__WEBPACK_IMPORTED_MODULE_2__["addRegexToken"])('DD', _parse_regex__WEBPACK_IMPORTED_MODULE_2__["match1to2"], _parse_regex__WEBPACK_IMPORTED_MODULE_2__["match2"]);
Object(_parse_regex__WEBPACK_IMPORTED_MODULE_2__["addRegexToken"])('Do', function (isStrict, locale) {
    return locale._dayOfMonthOrdinalParse || locale._ordinalParse;
});
Object(_parse_token__WEBPACK_IMPORTED_MODULE_3__["addParseToken"])(['D', 'DD'], _constants__WEBPACK_IMPORTED_MODULE_4__["DATE"]);
Object(_parse_token__WEBPACK_IMPORTED_MODULE_3__["addParseToken"])('Do', function (input, array, config) {
    array[_constants__WEBPACK_IMPORTED_MODULE_4__["DATE"]] = Object(_utils_type_checks__WEBPACK_IMPORTED_MODULE_5__["toInt"])(input.match(_parse_regex__WEBPACK_IMPORTED_MODULE_2__["match1to2"])[0]);
    return config;
});
//# sourceMappingURL=day-of-month.js.map

/***/ }),

/***/ "./node_modules/ngx-bootstrap/chronos/units/day-of-week.js":
/*!*****************************************************************!*\
  !*** ./node_modules/ngx-bootstrap/chronos/units/day-of-week.js ***!
  \*****************************************************************/
/*! exports provided: parseWeekday, parseIsoWeekday, getSetDayOfWeek, setDayOfWeek, getDayOfWeek, getLocaleDayOfWeek, setLocaleDayOfWeek, getISODayOfWeek, setISODayOfWeek */
/***/ (function(module, __webpack_exports__, __webpack_require__) {

"use strict";
__webpack_require__.r(__webpack_exports__);
/* harmony export (binding) */ __webpack_require__.d(__webpack_exports__, "parseWeekday", function() { return parseWeekday; });
/* harmony export (binding) */ __webpack_require__.d(__webpack_exports__, "parseIsoWeekday", function() { return parseIsoWeekday; });
/* harmony export (binding) */ __webpack_require__.d(__webpack_exports__, "getSetDayOfWeek", function() { return getSetDayOfWeek; });
/* harmony export (binding) */ __webpack_require__.d(__webpack_exports__, "setDayOfWeek", function() { return setDayOfWeek; });
/* harmony export (binding) */ __webpack_require__.d(__webpack_exports__, "getDayOfWeek", function() { return getDayOfWeek; });
/* harmony export (binding) */ __webpack_require__.d(__webpack_exports__, "getLocaleDayOfWeek", function() { return getLocaleDayOfWeek; });
/* harmony export (binding) */ __webpack_require__.d(__webpack_exports__, "setLocaleDayOfWeek", function() { return setLocaleDayOfWeek; });
/* harmony export (binding) */ __webpack_require__.d(__webpack_exports__, "getISODayOfWeek", function() { return getISODayOfWeek; });
/* harmony export (binding) */ __webpack_require__.d(__webpack_exports__, "setISODayOfWeek", function() { return setISODayOfWeek; });
/* harmony import */ var _format_format__WEBPACK_IMPORTED_MODULE_0__ = __webpack_require__(/*! ../format/format */ "./node_modules/ngx-bootstrap/chronos/format/format.js");
/* harmony import */ var _utils_date_getters__WEBPACK_IMPORTED_MODULE_1__ = __webpack_require__(/*! ../utils/date-getters */ "./node_modules/ngx-bootstrap/chronos/utils/date-getters.js");
/* harmony import */ var _parse_regex__WEBPACK_IMPORTED_MODULE_2__ = __webpack_require__(/*! ../parse/regex */ "./node_modules/ngx-bootstrap/chronos/parse/regex.js");
/* harmony import */ var _aliases__WEBPACK_IMPORTED_MODULE_3__ = __webpack_require__(/*! ./aliases */ "./node_modules/ngx-bootstrap/chronos/units/aliases.js");
/* harmony import */ var _priorities__WEBPACK_IMPORTED_MODULE_4__ = __webpack_require__(/*! ./priorities */ "./node_modules/ngx-bootstrap/chronos/units/priorities.js");
/* harmony import */ var _parse_token__WEBPACK_IMPORTED_MODULE_5__ = __webpack_require__(/*! ../parse/token */ "./node_modules/ngx-bootstrap/chronos/parse/token.js");
/* harmony import */ var _create_parsing_flags__WEBPACK_IMPORTED_MODULE_6__ = __webpack_require__(/*! ../create/parsing-flags */ "./node_modules/ngx-bootstrap/chronos/create/parsing-flags.js");
/* harmony import */ var _utils_type_checks__WEBPACK_IMPORTED_MODULE_7__ = __webpack_require__(/*! ../utils/type-checks */ "./node_modules/ngx-bootstrap/chronos/utils/type-checks.js");
/* harmony import */ var _moment_add_subtract__WEBPACK_IMPORTED_MODULE_8__ = __webpack_require__(/*! ../moment/add-subtract */ "./node_modules/ngx-bootstrap/chronos/moment/add-subtract.js");
/* harmony import */ var _locale_locales__WEBPACK_IMPORTED_MODULE_9__ = __webpack_require__(/*! ../locale/locales */ "./node_modules/ngx-bootstrap/chronos/locale/locales.js");










// FORMATTING
Object(_format_format__WEBPACK_IMPORTED_MODULE_0__["addFormatToken"])('d', null, 'do', function (date, opts) {
    return Object(_utils_date_getters__WEBPACK_IMPORTED_MODULE_1__["getDay"])(date, opts.isUTC).toString(10);
});
Object(_format_format__WEBPACK_IMPORTED_MODULE_0__["addFormatToken"])('dd', null, null, function (date, opts) {
    return opts.locale.weekdaysMin(date, opts.format, opts.isUTC);
});
Object(_format_format__WEBPACK_IMPORTED_MODULE_0__["addFormatToken"])('ddd', null, null, function (date, opts) {
    return opts.locale.weekdaysShort(date, opts.format, opts.isUTC);
});
Object(_format_format__WEBPACK_IMPORTED_MODULE_0__["addFormatToken"])('dddd', null, null, function (date, opts) {
    return opts.locale.weekdays(date, opts.format, opts.isUTC);
});
Object(_format_format__WEBPACK_IMPORTED_MODULE_0__["addFormatToken"])('e', null, null, function (date, opts) {
    return getLocaleDayOfWeek(date, opts.locale, opts.isUTC).toString(10);
    // return getDay(date, opts.isUTC).toString(10);
});
Object(_format_format__WEBPACK_IMPORTED_MODULE_0__["addFormatToken"])('E', null, null, function (date, opts) {
    return getISODayOfWeek(date, opts.isUTC).toString(10);
});
// ALIASES
Object(_aliases__WEBPACK_IMPORTED_MODULE_3__["addUnitAlias"])('day', 'd');
Object(_aliases__WEBPACK_IMPORTED_MODULE_3__["addUnitAlias"])('weekday', 'e');
Object(_aliases__WEBPACK_IMPORTED_MODULE_3__["addUnitAlias"])('isoWeekday', 'E');
// PRIORITY
Object(_priorities__WEBPACK_IMPORTED_MODULE_4__["addUnitPriority"])('day', 11);
Object(_priorities__WEBPACK_IMPORTED_MODULE_4__["addUnitPriority"])('weekday', 11);
Object(_priorities__WEBPACK_IMPORTED_MODULE_4__["addUnitPriority"])('isoWeekday', 11);
// PARSING
Object(_parse_regex__WEBPACK_IMPORTED_MODULE_2__["addRegexToken"])('d', _parse_regex__WEBPACK_IMPORTED_MODULE_2__["match1to2"]);
Object(_parse_regex__WEBPACK_IMPORTED_MODULE_2__["addRegexToken"])('e', _parse_regex__WEBPACK_IMPORTED_MODULE_2__["match1to2"]);
Object(_parse_regex__WEBPACK_IMPORTED_MODULE_2__["addRegexToken"])('E', _parse_regex__WEBPACK_IMPORTED_MODULE_2__["match1to2"]);
Object(_parse_regex__WEBPACK_IMPORTED_MODULE_2__["addRegexToken"])('dd', function (isStrict, locale) {
    return locale.weekdaysMinRegex(isStrict);
});
Object(_parse_regex__WEBPACK_IMPORTED_MODULE_2__["addRegexToken"])('ddd', function (isStrict, locale) {
    return locale.weekdaysShortRegex(isStrict);
});
Object(_parse_regex__WEBPACK_IMPORTED_MODULE_2__["addRegexToken"])('dddd', function (isStrict, locale) {
    return locale.weekdaysRegex(isStrict);
});
Object(_parse_token__WEBPACK_IMPORTED_MODULE_5__["addWeekParseToken"])(['dd', 'ddd', 'dddd'], function (input, week, config, token) {
    var weekday = config._locale.weekdaysParse(input, token, config._strict);
    // if we didn't get a weekday name, mark the date as invalid
    if (weekday != null) {
        week.d = weekday;
    }
    else {
        Object(_create_parsing_flags__WEBPACK_IMPORTED_MODULE_6__["getParsingFlags"])(config).invalidWeekday = !!input;
    }
    return config;
});
Object(_parse_token__WEBPACK_IMPORTED_MODULE_5__["addWeekParseToken"])(['d', 'e', 'E'], function (input, week, config, token) {
    week[token] = Object(_utils_type_checks__WEBPACK_IMPORTED_MODULE_7__["toInt"])(input);
    return config;
});
// HELPERS
function parseWeekday(input, locale) {
    if (!Object(_utils_type_checks__WEBPACK_IMPORTED_MODULE_7__["isString"])(input)) {
        return input;
    }
    var _num = parseInt(input, 10);
    if (!isNaN(_num)) {
        return _num;
    }
    var _weekDay = locale.weekdaysParse(input);
    if (Object(_utils_type_checks__WEBPACK_IMPORTED_MODULE_7__["isNumber"])(_weekDay)) {
        return _weekDay;
    }
    return null;
}
function parseIsoWeekday(input, locale) {
    if (locale === void 0) { locale = Object(_locale_locales__WEBPACK_IMPORTED_MODULE_9__["getLocale"])(); }
    if (Object(_utils_type_checks__WEBPACK_IMPORTED_MODULE_7__["isString"])(input)) {
        return locale.weekdaysParse(input) % 7 || 7;
    }
    return Object(_utils_type_checks__WEBPACK_IMPORTED_MODULE_7__["isNumber"])(input) && isNaN(input) ? null : input;
}
// MOMENTS
function getSetDayOfWeek(date, input, opts) {
    if (!input) {
        return getDayOfWeek(date, opts.isUTC);
    }
    return setDayOfWeek(date, input, opts.locale, opts.isUTC);
}
function setDayOfWeek(date, input, locale, isUTC) {
    if (locale === void 0) { locale = Object(_locale_locales__WEBPACK_IMPORTED_MODULE_9__["getLocale"])(); }
    var day = Object(_utils_date_getters__WEBPACK_IMPORTED_MODULE_1__["getDay"])(date, isUTC);
    var _input = parseWeekday(input, locale);
    return Object(_moment_add_subtract__WEBPACK_IMPORTED_MODULE_8__["add"])(date, _input - day, 'day');
}
function getDayOfWeek(date, isUTC) {
    return Object(_utils_date_getters__WEBPACK_IMPORTED_MODULE_1__["getDay"])(date, isUTC);
}
/********************************************/
// todo: utc
// getSetLocaleDayOfWeek
function getLocaleDayOfWeek(date, locale, isUTC) {
    if (locale === void 0) { locale = Object(_locale_locales__WEBPACK_IMPORTED_MODULE_9__["getLocale"])(); }
    return (Object(_utils_date_getters__WEBPACK_IMPORTED_MODULE_1__["getDay"])(date, isUTC) + 7 - locale.firstDayOfWeek()) % 7;
}
function setLocaleDayOfWeek(date, input, opts) {
    if (opts === void 0) { opts = {}; }
    var weekday = getLocaleDayOfWeek(date, opts.locale, opts.isUTC);
    return Object(_moment_add_subtract__WEBPACK_IMPORTED_MODULE_8__["add"])(date, input - weekday, 'day');
}
// getSetISODayOfWeek
function getISODayOfWeek(date, isUTC) {
    return Object(_utils_date_getters__WEBPACK_IMPORTED_MODULE_1__["getDay"])(date, isUTC) || 7;
}
function setISODayOfWeek(date, input, opts) {
    if (opts === void 0) { opts = {}; }
    // behaves the same as moment#day except
    // as a getter, returns 7 instead of 0 (1-7 range instead of 0-6)
    // as a setter, sunday should belong to the previous week.
    var weekday = parseIsoWeekday(input, opts.locale);
    return setDayOfWeek(date, getDayOfWeek(date) % 7 ? weekday : weekday - 7);
}
//# sourceMappingURL=day-of-week.js.map

/***/ }),

/***/ "./node_modules/ngx-bootstrap/chronos/units/day-of-year.js":
/*!*****************************************************************!*\
  !*** ./node_modules/ngx-bootstrap/chronos/units/day-of-year.js ***!
  \*****************************************************************/
/*! exports provided: getDayOfYear, setDayOfYear */
/***/ (function(module, __webpack_exports__, __webpack_require__) {

"use strict";
__webpack_require__.r(__webpack_exports__);
/* harmony export (binding) */ __webpack_require__.d(__webpack_exports__, "getDayOfYear", function() { return getDayOfYear; });
/* harmony export (binding) */ __webpack_require__.d(__webpack_exports__, "setDayOfYear", function() { return setDayOfYear; });
/* harmony import */ var _format_format__WEBPACK_IMPORTED_MODULE_0__ = __webpack_require__(/*! ../format/format */ "./node_modules/ngx-bootstrap/chronos/format/format.js");
/* harmony import */ var _utils_start_end_of__WEBPACK_IMPORTED_MODULE_1__ = __webpack_require__(/*! ../utils/start-end-of */ "./node_modules/ngx-bootstrap/chronos/utils/start-end-of.js");
/* harmony import */ var _parse_regex__WEBPACK_IMPORTED_MODULE_2__ = __webpack_require__(/*! ../parse/regex */ "./node_modules/ngx-bootstrap/chronos/parse/regex.js");
/* harmony import */ var _parse_token__WEBPACK_IMPORTED_MODULE_3__ = __webpack_require__(/*! ../parse/token */ "./node_modules/ngx-bootstrap/chronos/parse/token.js");
/* harmony import */ var _priorities__WEBPACK_IMPORTED_MODULE_4__ = __webpack_require__(/*! ./priorities */ "./node_modules/ngx-bootstrap/chronos/units/priorities.js");
/* harmony import */ var _aliases__WEBPACK_IMPORTED_MODULE_5__ = __webpack_require__(/*! ./aliases */ "./node_modules/ngx-bootstrap/chronos/units/aliases.js");
/* harmony import */ var _utils_type_checks__WEBPACK_IMPORTED_MODULE_6__ = __webpack_require__(/*! ../utils/type-checks */ "./node_modules/ngx-bootstrap/chronos/utils/type-checks.js");
/* harmony import */ var _moment_add_subtract__WEBPACK_IMPORTED_MODULE_7__ = __webpack_require__(/*! ../moment/add-subtract */ "./node_modules/ngx-bootstrap/chronos/moment/add-subtract.js");








// FORMATTING
Object(_format_format__WEBPACK_IMPORTED_MODULE_0__["addFormatToken"])('DDD', ['DDDD', 3, false], 'DDDo', function (date) {
    return getDayOfYear(date).toString(10);
});
// ALIASES
Object(_aliases__WEBPACK_IMPORTED_MODULE_5__["addUnitAlias"])('dayOfYear', 'DDD');
// PRIORITY
Object(_priorities__WEBPACK_IMPORTED_MODULE_4__["addUnitPriority"])('dayOfYear', 4);
Object(_parse_regex__WEBPACK_IMPORTED_MODULE_2__["addRegexToken"])('DDD', _parse_regex__WEBPACK_IMPORTED_MODULE_2__["match1to3"]);
Object(_parse_regex__WEBPACK_IMPORTED_MODULE_2__["addRegexToken"])('DDDD', _parse_regex__WEBPACK_IMPORTED_MODULE_2__["match3"]);
Object(_parse_token__WEBPACK_IMPORTED_MODULE_3__["addParseToken"])(['DDD', 'DDDD'], function (input, array, config) {
    config._dayOfYear = Object(_utils_type_checks__WEBPACK_IMPORTED_MODULE_6__["toInt"])(input);
    return config;
});
function getDayOfYear(date, isUTC) {
    var date1 = +Object(_utils_start_end_of__WEBPACK_IMPORTED_MODULE_1__["startOf"])(date, 'day', isUTC);
    var date2 = +Object(_utils_start_end_of__WEBPACK_IMPORTED_MODULE_1__["startOf"])(date, 'year', isUTC);
    var someDate = date1 - date2;
    var oneDay = 1000 * 60 * 60 * 24;
    return Math.round(someDate / oneDay) + 1;
}
function setDayOfYear(date, input) {
    var dayOfYear = getDayOfYear(date);
    return Object(_moment_add_subtract__WEBPACK_IMPORTED_MODULE_7__["add"])(date, (input - dayOfYear), 'day');
}
//# sourceMappingURL=day-of-year.js.map

/***/ }),

/***/ "./node_modules/ngx-bootstrap/chronos/units/hour.js":
/*!**********************************************************!*\
  !*** ./node_modules/ngx-bootstrap/chronos/units/hour.js ***!
  \**********************************************************/
/*! no exports provided */
/***/ (function(module, __webpack_exports__, __webpack_require__) {

"use strict";
__webpack_require__.r(__webpack_exports__);
/* harmony import */ var _utils_date_getters__WEBPACK_IMPORTED_MODULE_0__ = __webpack_require__(/*! ../utils/date-getters */ "./node_modules/ngx-bootstrap/chronos/utils/date-getters.js");
/* harmony import */ var _format_format__WEBPACK_IMPORTED_MODULE_1__ = __webpack_require__(/*! ../format/format */ "./node_modules/ngx-bootstrap/chronos/format/format.js");
/* harmony import */ var _utils_zero_fill__WEBPACK_IMPORTED_MODULE_2__ = __webpack_require__(/*! ../utils/zero-fill */ "./node_modules/ngx-bootstrap/chronos/utils/zero-fill.js");
/* harmony import */ var _parse_regex__WEBPACK_IMPORTED_MODULE_3__ = __webpack_require__(/*! ../parse/regex */ "./node_modules/ngx-bootstrap/chronos/parse/regex.js");
/* harmony import */ var _parse_token__WEBPACK_IMPORTED_MODULE_4__ = __webpack_require__(/*! ../parse/token */ "./node_modules/ngx-bootstrap/chronos/parse/token.js");
/* harmony import */ var _constants__WEBPACK_IMPORTED_MODULE_5__ = __webpack_require__(/*! ./constants */ "./node_modules/ngx-bootstrap/chronos/units/constants.js");
/* harmony import */ var _utils_type_checks__WEBPACK_IMPORTED_MODULE_6__ = __webpack_require__(/*! ../utils/type-checks */ "./node_modules/ngx-bootstrap/chronos/utils/type-checks.js");
/* harmony import */ var _create_parsing_flags__WEBPACK_IMPORTED_MODULE_7__ = __webpack_require__(/*! ../create/parsing-flags */ "./node_modules/ngx-bootstrap/chronos/create/parsing-flags.js");
/* harmony import */ var _priorities__WEBPACK_IMPORTED_MODULE_8__ = __webpack_require__(/*! ./priorities */ "./node_modules/ngx-bootstrap/chronos/units/priorities.js");
/* harmony import */ var _aliases__WEBPACK_IMPORTED_MODULE_9__ = __webpack_require__(/*! ./aliases */ "./node_modules/ngx-bootstrap/chronos/units/aliases.js");










// FORMATTING
function hFormat(date, isUTC) {
    return Object(_utils_date_getters__WEBPACK_IMPORTED_MODULE_0__["getHours"])(date, isUTC) % 12 || 12;
}
function kFormat(date, isUTC) {
    return Object(_utils_date_getters__WEBPACK_IMPORTED_MODULE_0__["getHours"])(date, isUTC) || 24;
}
Object(_format_format__WEBPACK_IMPORTED_MODULE_1__["addFormatToken"])('H', ['HH', 2, false], null, function (date, opts) {
    return Object(_utils_date_getters__WEBPACK_IMPORTED_MODULE_0__["getHours"])(date, opts.isUTC).toString(10);
});
Object(_format_format__WEBPACK_IMPORTED_MODULE_1__["addFormatToken"])('h', ['hh', 2, false], null, function (date, opts) {
    return hFormat(date, opts.isUTC).toString(10);
});
Object(_format_format__WEBPACK_IMPORTED_MODULE_1__["addFormatToken"])('k', ['kk', 2, false], null, function (date, opts) {
    return kFormat(date, opts.isUTC).toString(10);
});
Object(_format_format__WEBPACK_IMPORTED_MODULE_1__["addFormatToken"])('hmm', null, null, function (date, opts) {
    var _h = hFormat(date, opts.isUTC);
    var _mm = Object(_utils_zero_fill__WEBPACK_IMPORTED_MODULE_2__["zeroFill"])(Object(_utils_date_getters__WEBPACK_IMPORTED_MODULE_0__["getMinutes"])(date, opts.isUTC), 2);
    return "" + _h + _mm;
});
Object(_format_format__WEBPACK_IMPORTED_MODULE_1__["addFormatToken"])('hmmss', null, null, function (date, opts) {
    var _h = hFormat(date, opts.isUTC);
    var _mm = Object(_utils_zero_fill__WEBPACK_IMPORTED_MODULE_2__["zeroFill"])(Object(_utils_date_getters__WEBPACK_IMPORTED_MODULE_0__["getMinutes"])(date, opts.isUTC), 2);
    var _ss = Object(_utils_zero_fill__WEBPACK_IMPORTED_MODULE_2__["zeroFill"])(Object(_utils_date_getters__WEBPACK_IMPORTED_MODULE_0__["getSeconds"])(date, opts.isUTC), 2);
    return "" + _h + _mm + _ss;
});
Object(_format_format__WEBPACK_IMPORTED_MODULE_1__["addFormatToken"])('Hmm', null, null, function (date, opts) {
    var _H = Object(_utils_date_getters__WEBPACK_IMPORTED_MODULE_0__["getHours"])(date, opts.isUTC);
    var _mm = Object(_utils_zero_fill__WEBPACK_IMPORTED_MODULE_2__["zeroFill"])(Object(_utils_date_getters__WEBPACK_IMPORTED_MODULE_0__["getMinutes"])(date, opts.isUTC), 2);
    return "" + _H + _mm;
});
Object(_format_format__WEBPACK_IMPORTED_MODULE_1__["addFormatToken"])('Hmmss', null, null, function (date, opts) {
    var _H = Object(_utils_date_getters__WEBPACK_IMPORTED_MODULE_0__["getHours"])(date, opts.isUTC);
    var _mm = Object(_utils_zero_fill__WEBPACK_IMPORTED_MODULE_2__["zeroFill"])(Object(_utils_date_getters__WEBPACK_IMPORTED_MODULE_0__["getMinutes"])(date, opts.isUTC), 2);
    var _ss = Object(_utils_zero_fill__WEBPACK_IMPORTED_MODULE_2__["zeroFill"])(Object(_utils_date_getters__WEBPACK_IMPORTED_MODULE_0__["getSeconds"])(date, opts.isUTC), 2);
    return "" + _H + _mm + _ss;
});
function meridiem(token, lowercase) {
    Object(_format_format__WEBPACK_IMPORTED_MODULE_1__["addFormatToken"])(token, null, null, function (date, opts) {
        return opts.locale.meridiem(Object(_utils_date_getters__WEBPACK_IMPORTED_MODULE_0__["getHours"])(date, opts.isUTC), Object(_utils_date_getters__WEBPACK_IMPORTED_MODULE_0__["getMinutes"])(date, opts.isUTC), lowercase);
    });
}
meridiem('a', true);
meridiem('A', false);
// ALIASES
Object(_aliases__WEBPACK_IMPORTED_MODULE_9__["addUnitAlias"])('hour', 'h');
// PRIORITY
Object(_priorities__WEBPACK_IMPORTED_MODULE_8__["addUnitPriority"])('hour', 13);
// PARSING
function matchMeridiem(isStrict, locale) {
    return locale._meridiemParse;
}
Object(_parse_regex__WEBPACK_IMPORTED_MODULE_3__["addRegexToken"])('a', matchMeridiem);
Object(_parse_regex__WEBPACK_IMPORTED_MODULE_3__["addRegexToken"])('A', matchMeridiem);
Object(_parse_regex__WEBPACK_IMPORTED_MODULE_3__["addRegexToken"])('H', _parse_regex__WEBPACK_IMPORTED_MODULE_3__["match1to2"]);
Object(_parse_regex__WEBPACK_IMPORTED_MODULE_3__["addRegexToken"])('h', _parse_regex__WEBPACK_IMPORTED_MODULE_3__["match1to2"]);
Object(_parse_regex__WEBPACK_IMPORTED_MODULE_3__["addRegexToken"])('k', _parse_regex__WEBPACK_IMPORTED_MODULE_3__["match1to2"]);
Object(_parse_regex__WEBPACK_IMPORTED_MODULE_3__["addRegexToken"])('HH', _parse_regex__WEBPACK_IMPORTED_MODULE_3__["match1to2"], _parse_regex__WEBPACK_IMPORTED_MODULE_3__["match2"]);
Object(_parse_regex__WEBPACK_IMPORTED_MODULE_3__["addRegexToken"])('hh', _parse_regex__WEBPACK_IMPORTED_MODULE_3__["match1to2"], _parse_regex__WEBPACK_IMPORTED_MODULE_3__["match2"]);
Object(_parse_regex__WEBPACK_IMPORTED_MODULE_3__["addRegexToken"])('kk', _parse_regex__WEBPACK_IMPORTED_MODULE_3__["match1to2"], _parse_regex__WEBPACK_IMPORTED_MODULE_3__["match2"]);
Object(_parse_regex__WEBPACK_IMPORTED_MODULE_3__["addRegexToken"])('hmm', _parse_regex__WEBPACK_IMPORTED_MODULE_3__["match3to4"]);
Object(_parse_regex__WEBPACK_IMPORTED_MODULE_3__["addRegexToken"])('hmmss', _parse_regex__WEBPACK_IMPORTED_MODULE_3__["match5to6"]);
Object(_parse_regex__WEBPACK_IMPORTED_MODULE_3__["addRegexToken"])('Hmm', _parse_regex__WEBPACK_IMPORTED_MODULE_3__["match3to4"]);
Object(_parse_regex__WEBPACK_IMPORTED_MODULE_3__["addRegexToken"])('Hmmss', _parse_regex__WEBPACK_IMPORTED_MODULE_3__["match5to6"]);
Object(_parse_token__WEBPACK_IMPORTED_MODULE_4__["addParseToken"])(['H', 'HH'], _constants__WEBPACK_IMPORTED_MODULE_5__["HOUR"]);
Object(_parse_token__WEBPACK_IMPORTED_MODULE_4__["addParseToken"])(['k', 'kk'], function (input, array, config) {
    var kInput = Object(_utils_type_checks__WEBPACK_IMPORTED_MODULE_6__["toInt"])(input);
    array[_constants__WEBPACK_IMPORTED_MODULE_5__["HOUR"]] = kInput === 24 ? 0 : kInput;
    return config;
});
Object(_parse_token__WEBPACK_IMPORTED_MODULE_4__["addParseToken"])(['a', 'A'], function (input, array, config) {
    config._isPm = config._locale.isPM(input);
    config._meridiem = input;
    return config;
});
Object(_parse_token__WEBPACK_IMPORTED_MODULE_4__["addParseToken"])(['h', 'hh'], function (input, array, config) {
    array[_constants__WEBPACK_IMPORTED_MODULE_5__["HOUR"]] = Object(_utils_type_checks__WEBPACK_IMPORTED_MODULE_6__["toInt"])(input);
    Object(_create_parsing_flags__WEBPACK_IMPORTED_MODULE_7__["getParsingFlags"])(config).bigHour = true;
    return config;
});
Object(_parse_token__WEBPACK_IMPORTED_MODULE_4__["addParseToken"])('hmm', function (input, array, config) {
    var pos = input.length - 2;
    array[_constants__WEBPACK_IMPORTED_MODULE_5__["HOUR"]] = Object(_utils_type_checks__WEBPACK_IMPORTED_MODULE_6__["toInt"])(input.substr(0, pos));
    array[_constants__WEBPACK_IMPORTED_MODULE_5__["MINUTE"]] = Object(_utils_type_checks__WEBPACK_IMPORTED_MODULE_6__["toInt"])(input.substr(pos));
    Object(_create_parsing_flags__WEBPACK_IMPORTED_MODULE_7__["getParsingFlags"])(config).bigHour = true;
    return config;
});
Object(_parse_token__WEBPACK_IMPORTED_MODULE_4__["addParseToken"])('hmmss', function (input, array, config) {
    var pos1 = input.length - 4;
    var pos2 = input.length - 2;
    array[_constants__WEBPACK_IMPORTED_MODULE_5__["HOUR"]] = Object(_utils_type_checks__WEBPACK_IMPORTED_MODULE_6__["toInt"])(input.substr(0, pos1));
    array[_constants__WEBPACK_IMPORTED_MODULE_5__["MINUTE"]] = Object(_utils_type_checks__WEBPACK_IMPORTED_MODULE_6__["toInt"])(input.substr(pos1, 2));
    array[_constants__WEBPACK_IMPORTED_MODULE_5__["SECOND"]] = Object(_utils_type_checks__WEBPACK_IMPORTED_MODULE_6__["toInt"])(input.substr(pos2));
    Object(_create_parsing_flags__WEBPACK_IMPORTED_MODULE_7__["getParsingFlags"])(config).bigHour = true;
    return config;
});
Object(_parse_token__WEBPACK_IMPORTED_MODULE_4__["addParseToken"])('Hmm', function (input, array, config) {
    var pos = input.length - 2;
    array[_constants__WEBPACK_IMPORTED_MODULE_5__["HOUR"]] = Object(_utils_type_checks__WEBPACK_IMPORTED_MODULE_6__["toInt"])(input.substr(0, pos));
    array[_constants__WEBPACK_IMPORTED_MODULE_5__["MINUTE"]] = Object(_utils_type_checks__WEBPACK_IMPORTED_MODULE_6__["toInt"])(input.substr(pos));
    return config;
});
Object(_parse_token__WEBPACK_IMPORTED_MODULE_4__["addParseToken"])('Hmmss', function (input, array, config) {
    var pos1 = input.length - 4;
    var pos2 = input.length - 2;
    array[_constants__WEBPACK_IMPORTED_MODULE_5__["HOUR"]] = Object(_utils_type_checks__WEBPACK_IMPORTED_MODULE_6__["toInt"])(input.substr(0, pos1));
    array[_constants__WEBPACK_IMPORTED_MODULE_5__["MINUTE"]] = Object(_utils_type_checks__WEBPACK_IMPORTED_MODULE_6__["toInt"])(input.substr(pos1, 2));
    array[_constants__WEBPACK_IMPORTED_MODULE_5__["SECOND"]] = Object(_utils_type_checks__WEBPACK_IMPORTED_MODULE_6__["toInt"])(input.substr(pos2));
    return config;
});
//# sourceMappingURL=hour.js.map

/***/ }),

/***/ "./node_modules/ngx-bootstrap/chronos/units/index.js":
/*!***********************************************************!*\
  !*** ./node_modules/ngx-bootstrap/chronos/units/index.js ***!
  \***********************************************************/
/*! no exports provided */
/***/ (function(module, __webpack_exports__, __webpack_require__) {

"use strict";
__webpack_require__.r(__webpack_exports__);
/* harmony import */ var _aliases__WEBPACK_IMPORTED_MODULE_0__ = __webpack_require__(/*! ./aliases */ "./node_modules/ngx-bootstrap/chronos/units/aliases.js");
/* harmony import */ var _constants__WEBPACK_IMPORTED_MODULE_1__ = __webpack_require__(/*! ./constants */ "./node_modules/ngx-bootstrap/chronos/units/constants.js");
/* harmony import */ var _day_of_month__WEBPACK_IMPORTED_MODULE_2__ = __webpack_require__(/*! ./day-of-month */ "./node_modules/ngx-bootstrap/chronos/units/day-of-month.js");
/* harmony import */ var _day_of_week__WEBPACK_IMPORTED_MODULE_3__ = __webpack_require__(/*! ./day-of-week */ "./node_modules/ngx-bootstrap/chronos/units/day-of-week.js");
/* harmony import */ var _day_of_year__WEBPACK_IMPORTED_MODULE_4__ = __webpack_require__(/*! ./day-of-year */ "./node_modules/ngx-bootstrap/chronos/units/day-of-year.js");
/* harmony import */ var _hour__WEBPACK_IMPORTED_MODULE_5__ = __webpack_require__(/*! ./hour */ "./node_modules/ngx-bootstrap/chronos/units/hour.js");
/* harmony import */ var _millisecond__WEBPACK_IMPORTED_MODULE_6__ = __webpack_require__(/*! ./millisecond */ "./node_modules/ngx-bootstrap/chronos/units/millisecond.js");
/* harmony import */ var _minute__WEBPACK_IMPORTED_MODULE_7__ = __webpack_require__(/*! ./minute */ "./node_modules/ngx-bootstrap/chronos/units/minute.js");
/* harmony import */ var _month__WEBPACK_IMPORTED_MODULE_8__ = __webpack_require__(/*! ./month */ "./node_modules/ngx-bootstrap/chronos/units/month.js");
/* harmony import */ var _offset__WEBPACK_IMPORTED_MODULE_9__ = __webpack_require__(/*! ./offset */ "./node_modules/ngx-bootstrap/chronos/units/offset.js");
/* harmony import */ var _priorities__WEBPACK_IMPORTED_MODULE_10__ = __webpack_require__(/*! ./priorities */ "./node_modules/ngx-bootstrap/chronos/units/priorities.js");
/* harmony import */ var _quarter__WEBPACK_IMPORTED_MODULE_11__ = __webpack_require__(/*! ./quarter */ "./node_modules/ngx-bootstrap/chronos/units/quarter.js");
/* harmony import */ var _second__WEBPACK_IMPORTED_MODULE_12__ = __webpack_require__(/*! ./second */ "./node_modules/ngx-bootstrap/chronos/units/second.js");
/* harmony import */ var _timestamp__WEBPACK_IMPORTED_MODULE_13__ = __webpack_require__(/*! ./timestamp */ "./node_modules/ngx-bootstrap/chronos/units/timestamp.js");
/* harmony import */ var _week__WEBPACK_IMPORTED_MODULE_14__ = __webpack_require__(/*! ./week */ "./node_modules/ngx-bootstrap/chronos/units/week.js");
/* harmony import */ var _week_calendar_utils__WEBPACK_IMPORTED_MODULE_15__ = __webpack_require__(/*! ./week-calendar-utils */ "./node_modules/ngx-bootstrap/chronos/units/week-calendar-utils.js");
/* harmony import */ var _week_year__WEBPACK_IMPORTED_MODULE_16__ = __webpack_require__(/*! ./week-year */ "./node_modules/ngx-bootstrap/chronos/units/week-year.js");
/* harmony import */ var _year__WEBPACK_IMPORTED_MODULE_17__ = __webpack_require__(/*! ./year */ "./node_modules/ngx-bootstrap/chronos/units/year.js");


















//# sourceMappingURL=index.js.map

/***/ }),

/***/ "./node_modules/ngx-bootstrap/chronos/units/millisecond.js":
/*!*****************************************************************!*\
  !*** ./node_modules/ngx-bootstrap/chronos/units/millisecond.js ***!
  \*****************************************************************/
/*! no exports provided */
/***/ (function(module, __webpack_exports__, __webpack_require__) {

"use strict";
__webpack_require__.r(__webpack_exports__);
/* harmony import */ var _format_format__WEBPACK_IMPORTED_MODULE_0__ = __webpack_require__(/*! ../format/format */ "./node_modules/ngx-bootstrap/chronos/format/format.js");
/* harmony import */ var _parse_regex__WEBPACK_IMPORTED_MODULE_1__ = __webpack_require__(/*! ../parse/regex */ "./node_modules/ngx-bootstrap/chronos/parse/regex.js");
/* harmony import */ var _constants__WEBPACK_IMPORTED_MODULE_2__ = __webpack_require__(/*! ./constants */ "./node_modules/ngx-bootstrap/chronos/units/constants.js");
/* harmony import */ var _utils_type_checks__WEBPACK_IMPORTED_MODULE_3__ = __webpack_require__(/*! ../utils/type-checks */ "./node_modules/ngx-bootstrap/chronos/utils/type-checks.js");
/* harmony import */ var _parse_token__WEBPACK_IMPORTED_MODULE_4__ = __webpack_require__(/*! ../parse/token */ "./node_modules/ngx-bootstrap/chronos/parse/token.js");
/* harmony import */ var _aliases__WEBPACK_IMPORTED_MODULE_5__ = __webpack_require__(/*! ./aliases */ "./node_modules/ngx-bootstrap/chronos/units/aliases.js");
/* harmony import */ var _priorities__WEBPACK_IMPORTED_MODULE_6__ = __webpack_require__(/*! ./priorities */ "./node_modules/ngx-bootstrap/chronos/units/priorities.js");
/* harmony import */ var _utils_date_getters__WEBPACK_IMPORTED_MODULE_7__ = __webpack_require__(/*! ../utils/date-getters */ "./node_modules/ngx-bootstrap/chronos/utils/date-getters.js");
// tslint:disable:no-bitwise
// FORMATTING








Object(_format_format__WEBPACK_IMPORTED_MODULE_0__["addFormatToken"])('S', null, null, function (date, opts) {
    return (~~(Object(_utils_date_getters__WEBPACK_IMPORTED_MODULE_7__["getMilliseconds"])(date, opts.isUTC) / 100)).toString(10);
});
Object(_format_format__WEBPACK_IMPORTED_MODULE_0__["addFormatToken"])(null, ['SS', 2, false], null, function (date, opts) {
    return (~~(Object(_utils_date_getters__WEBPACK_IMPORTED_MODULE_7__["getMilliseconds"])(date, opts.isUTC) / 10)).toString(10);
});
Object(_format_format__WEBPACK_IMPORTED_MODULE_0__["addFormatToken"])(null, ['SSS', 3, false], null, function (date, opts) {
    return (Object(_utils_date_getters__WEBPACK_IMPORTED_MODULE_7__["getMilliseconds"])(date, opts.isUTC)).toString(10);
});
Object(_format_format__WEBPACK_IMPORTED_MODULE_0__["addFormatToken"])(null, ['SSSS', 4, false], null, function (date, opts) {
    return (Object(_utils_date_getters__WEBPACK_IMPORTED_MODULE_7__["getMilliseconds"])(date, opts.isUTC) * 10).toString(10);
});
Object(_format_format__WEBPACK_IMPORTED_MODULE_0__["addFormatToken"])(null, ['SSSSS', 5, false], null, function (date, opts) {
    return (Object(_utils_date_getters__WEBPACK_IMPORTED_MODULE_7__["getMilliseconds"])(date, opts.isUTC) * 100).toString(10);
});
Object(_format_format__WEBPACK_IMPORTED_MODULE_0__["addFormatToken"])(null, ['SSSSSS', 6, false], null, function (date, opts) {
    return (Object(_utils_date_getters__WEBPACK_IMPORTED_MODULE_7__["getMilliseconds"])(date, opts.isUTC) * 1000).toString(10);
});
Object(_format_format__WEBPACK_IMPORTED_MODULE_0__["addFormatToken"])(null, ['SSSSSSS', 7, false], null, function (date, opts) {
    return (Object(_utils_date_getters__WEBPACK_IMPORTED_MODULE_7__["getMilliseconds"])(date, opts.isUTC) * 10000).toString(10);
});
Object(_format_format__WEBPACK_IMPORTED_MODULE_0__["addFormatToken"])(null, ['SSSSSSSS', 8, false], null, function (date, opts) {
    return (Object(_utils_date_getters__WEBPACK_IMPORTED_MODULE_7__["getMilliseconds"])(date, opts.isUTC) * 100000).toString(10);
});
Object(_format_format__WEBPACK_IMPORTED_MODULE_0__["addFormatToken"])(null, ['SSSSSSSSS', 9, false], null, function (date, opts) {
    return (Object(_utils_date_getters__WEBPACK_IMPORTED_MODULE_7__["getMilliseconds"])(date, opts.isUTC) * 1000000).toString(10);
});
// ALIASES
Object(_aliases__WEBPACK_IMPORTED_MODULE_5__["addUnitAlias"])('millisecond', 'ms');
// PRIORITY
Object(_priorities__WEBPACK_IMPORTED_MODULE_6__["addUnitPriority"])('millisecond', 16);
// PARSING
Object(_parse_regex__WEBPACK_IMPORTED_MODULE_1__["addRegexToken"])('S', _parse_regex__WEBPACK_IMPORTED_MODULE_1__["match1to3"], _parse_regex__WEBPACK_IMPORTED_MODULE_1__["match1"]);
Object(_parse_regex__WEBPACK_IMPORTED_MODULE_1__["addRegexToken"])('SS', _parse_regex__WEBPACK_IMPORTED_MODULE_1__["match1to3"], _parse_regex__WEBPACK_IMPORTED_MODULE_1__["match2"]);
Object(_parse_regex__WEBPACK_IMPORTED_MODULE_1__["addRegexToken"])('SSS', _parse_regex__WEBPACK_IMPORTED_MODULE_1__["match1to3"], _parse_regex__WEBPACK_IMPORTED_MODULE_1__["match3"]);
var token;
for (token = 'SSSS'; token.length <= 9; token += 'S') {
    Object(_parse_regex__WEBPACK_IMPORTED_MODULE_1__["addRegexToken"])(token, _parse_regex__WEBPACK_IMPORTED_MODULE_1__["matchUnsigned"]);
}
function parseMs(input, array, config) {
    array[_constants__WEBPACK_IMPORTED_MODULE_2__["MILLISECOND"]] = Object(_utils_type_checks__WEBPACK_IMPORTED_MODULE_3__["toInt"])(parseFloat("0." + input) * 1000);
    return config;
}
for (token = 'S'; token.length <= 9; token += 'S') {
    Object(_parse_token__WEBPACK_IMPORTED_MODULE_4__["addParseToken"])(token, parseMs);
}
// MOMENTS
//# sourceMappingURL=millisecond.js.map

/***/ }),

/***/ "./node_modules/ngx-bootstrap/chronos/units/minute.js":
/*!************************************************************!*\
  !*** ./node_modules/ngx-bootstrap/chronos/units/minute.js ***!
  \************************************************************/
/*! no exports provided */
/***/ (function(module, __webpack_exports__, __webpack_require__) {

"use strict";
__webpack_require__.r(__webpack_exports__);
/* harmony import */ var _format_format__WEBPACK_IMPORTED_MODULE_0__ = __webpack_require__(/*! ../format/format */ "./node_modules/ngx-bootstrap/chronos/format/format.js");
/* harmony import */ var _utils_date_getters__WEBPACK_IMPORTED_MODULE_1__ = __webpack_require__(/*! ../utils/date-getters */ "./node_modules/ngx-bootstrap/chronos/utils/date-getters.js");
/* harmony import */ var _parse_regex__WEBPACK_IMPORTED_MODULE_2__ = __webpack_require__(/*! ../parse/regex */ "./node_modules/ngx-bootstrap/chronos/parse/regex.js");
/* harmony import */ var _parse_token__WEBPACK_IMPORTED_MODULE_3__ = __webpack_require__(/*! ../parse/token */ "./node_modules/ngx-bootstrap/chronos/parse/token.js");
/* harmony import */ var _constants__WEBPACK_IMPORTED_MODULE_4__ = __webpack_require__(/*! ./constants */ "./node_modules/ngx-bootstrap/chronos/units/constants.js");
/* harmony import */ var _priorities__WEBPACK_IMPORTED_MODULE_5__ = __webpack_require__(/*! ./priorities */ "./node_modules/ngx-bootstrap/chronos/units/priorities.js");
/* harmony import */ var _aliases__WEBPACK_IMPORTED_MODULE_6__ = __webpack_require__(/*! ./aliases */ "./node_modules/ngx-bootstrap/chronos/units/aliases.js");







// FORMATTING
Object(_format_format__WEBPACK_IMPORTED_MODULE_0__["addFormatToken"])('m', ['mm', 2, false], null, function (date, opts) {
    return Object(_utils_date_getters__WEBPACK_IMPORTED_MODULE_1__["getMinutes"])(date, opts.isUTC).toString(10);
});
// ALIASES
Object(_aliases__WEBPACK_IMPORTED_MODULE_6__["addUnitAlias"])('minute', 'm');
// PRIORITY
Object(_priorities__WEBPACK_IMPORTED_MODULE_5__["addUnitPriority"])('minute', 14);
// PARSING
Object(_parse_regex__WEBPACK_IMPORTED_MODULE_2__["addRegexToken"])('m', _parse_regex__WEBPACK_IMPORTED_MODULE_2__["match1to2"]);
Object(_parse_regex__WEBPACK_IMPORTED_MODULE_2__["addRegexToken"])('mm', _parse_regex__WEBPACK_IMPORTED_MODULE_2__["match1to2"], _parse_regex__WEBPACK_IMPORTED_MODULE_2__["match2"]);
Object(_parse_token__WEBPACK_IMPORTED_MODULE_3__["addParseToken"])(['m', 'mm'], _constants__WEBPACK_IMPORTED_MODULE_4__["MINUTE"]);
//# sourceMappingURL=minute.js.map

/***/ }),

/***/ "./node_modules/ngx-bootstrap/chronos/units/month.js":
/*!***********************************************************!*\
  !*** ./node_modules/ngx-bootstrap/chronos/units/month.js ***!
  \***********************************************************/
/*! exports provided: daysInMonth */
/***/ (function(module, __webpack_exports__, __webpack_require__) {

"use strict";
__webpack_require__.r(__webpack_exports__);
/* harmony export (binding) */ __webpack_require__.d(__webpack_exports__, "daysInMonth", function() { return daysInMonth; });
/* harmony import */ var _format_format__WEBPACK_IMPORTED_MODULE_0__ = __webpack_require__(/*! ../format/format */ "./node_modules/ngx-bootstrap/chronos/format/format.js");
/* harmony import */ var _year__WEBPACK_IMPORTED_MODULE_1__ = __webpack_require__(/*! ./year */ "./node_modules/ngx-bootstrap/chronos/units/year.js");
/* harmony import */ var _utils__WEBPACK_IMPORTED_MODULE_2__ = __webpack_require__(/*! ../utils */ "./node_modules/ngx-bootstrap/chronos/utils.js");
/* harmony import */ var _utils_date_getters__WEBPACK_IMPORTED_MODULE_3__ = __webpack_require__(/*! ../utils/date-getters */ "./node_modules/ngx-bootstrap/chronos/utils/date-getters.js");
/* harmony import */ var _parse_regex__WEBPACK_IMPORTED_MODULE_4__ = __webpack_require__(/*! ../parse/regex */ "./node_modules/ngx-bootstrap/chronos/parse/regex.js");
/* harmony import */ var _parse_token__WEBPACK_IMPORTED_MODULE_5__ = __webpack_require__(/*! ../parse/token */ "./node_modules/ngx-bootstrap/chronos/parse/token.js");
/* harmony import */ var _constants__WEBPACK_IMPORTED_MODULE_6__ = __webpack_require__(/*! ./constants */ "./node_modules/ngx-bootstrap/chronos/units/constants.js");
/* harmony import */ var _utils_type_checks__WEBPACK_IMPORTED_MODULE_7__ = __webpack_require__(/*! ../utils/type-checks */ "./node_modules/ngx-bootstrap/chronos/utils/type-checks.js");
/* harmony import */ var _priorities__WEBPACK_IMPORTED_MODULE_8__ = __webpack_require__(/*! ./priorities */ "./node_modules/ngx-bootstrap/chronos/units/priorities.js");
/* harmony import */ var _aliases__WEBPACK_IMPORTED_MODULE_9__ = __webpack_require__(/*! ./aliases */ "./node_modules/ngx-bootstrap/chronos/units/aliases.js");
/* harmony import */ var _create_parsing_flags__WEBPACK_IMPORTED_MODULE_10__ = __webpack_require__(/*! ../create/parsing-flags */ "./node_modules/ngx-bootstrap/chronos/create/parsing-flags.js");











// todo: this is duplicate, source in date-getters.ts
function daysInMonth(year, month) {
    if (isNaN(year) || isNaN(month)) {
        return NaN;
    }
    var modMonth = Object(_utils__WEBPACK_IMPORTED_MODULE_2__["mod"])(month, 12);
    var _year = year + (month - modMonth) / 12;
    return modMonth === 1
        ? Object(_year__WEBPACK_IMPORTED_MODULE_1__["isLeapYear"])(_year) ? 29 : 28
        : (31 - modMonth % 7 % 2);
}
// FORMATTING
Object(_format_format__WEBPACK_IMPORTED_MODULE_0__["addFormatToken"])('M', ['MM', 2, false], 'Mo', function (date, opts) {
    return (Object(_utils_date_getters__WEBPACK_IMPORTED_MODULE_3__["getMonth"])(date, opts.isUTC) + 1).toString(10);
});
Object(_format_format__WEBPACK_IMPORTED_MODULE_0__["addFormatToken"])('MMM', null, null, function (date, opts) {
    return opts.locale.monthsShort(date, opts.format, opts.isUTC);
});
Object(_format_format__WEBPACK_IMPORTED_MODULE_0__["addFormatToken"])('MMMM', null, null, function (date, opts) {
    return opts.locale.months(date, opts.format, opts.isUTC);
});
// ALIASES
Object(_aliases__WEBPACK_IMPORTED_MODULE_9__["addUnitAlias"])('month', 'M');
// PRIORITY
Object(_priorities__WEBPACK_IMPORTED_MODULE_8__["addUnitPriority"])('month', 8);
// PARSING
Object(_parse_regex__WEBPACK_IMPORTED_MODULE_4__["addRegexToken"])('M', _parse_regex__WEBPACK_IMPORTED_MODULE_4__["match1to2"]);
Object(_parse_regex__WEBPACK_IMPORTED_MODULE_4__["addRegexToken"])('MM', _parse_regex__WEBPACK_IMPORTED_MODULE_4__["match1to2"], _parse_regex__WEBPACK_IMPORTED_MODULE_4__["match2"]);
Object(_parse_regex__WEBPACK_IMPORTED_MODULE_4__["addRegexToken"])('MMM', function (isStrict, locale) {
    return locale.monthsShortRegex(isStrict);
});
Object(_parse_regex__WEBPACK_IMPORTED_MODULE_4__["addRegexToken"])('MMMM', function (isStrict, locale) {
    return locale.monthsRegex(isStrict);
});
Object(_parse_token__WEBPACK_IMPORTED_MODULE_5__["addParseToken"])(['M', 'MM'], function (input, array, config) {
    array[_constants__WEBPACK_IMPORTED_MODULE_6__["MONTH"]] = Object(_utils_type_checks__WEBPACK_IMPORTED_MODULE_7__["toInt"])(input) - 1;
    return config;
});
Object(_parse_token__WEBPACK_IMPORTED_MODULE_5__["addParseToken"])(['MMM', 'MMMM'], function (input, array, config, token) {
    var month = config._locale.monthsParse(input, token, config._strict);
    // if we didn't find a month name, mark the date as invalid.
    if (month != null) {
        array[_constants__WEBPACK_IMPORTED_MODULE_6__["MONTH"]] = month;
    }
    else {
        Object(_create_parsing_flags__WEBPACK_IMPORTED_MODULE_10__["getParsingFlags"])(config).invalidMonth = !!input;
    }
    return config;
});
//# sourceMappingURL=month.js.map

/***/ }),

/***/ "./node_modules/ngx-bootstrap/chronos/units/offset.js":
/*!************************************************************!*\
  !*** ./node_modules/ngx-bootstrap/chronos/units/offset.js ***!
  \************************************************************/
/*! exports provided: cloneWithOffset, getDateOffset, getUTCOffset, setUTCOffset, setOffsetToUTC, isDaylightSavingTime, setOffsetToParsedOffset, hasAlignedHourOffset */
/***/ (function(module, __webpack_exports__, __webpack_require__) {

"use strict";
__webpack_require__.r(__webpack_exports__);
/* harmony export (binding) */ __webpack_require__.d(__webpack_exports__, "cloneWithOffset", function() { return cloneWithOffset; });
/* harmony export (binding) */ __webpack_require__.d(__webpack_exports__, "getDateOffset", function() { return getDateOffset; });
/* harmony export (binding) */ __webpack_require__.d(__webpack_exports__, "getUTCOffset", function() { return getUTCOffset; });
/* harmony export (binding) */ __webpack_require__.d(__webpack_exports__, "setUTCOffset", function() { return setUTCOffset; });
/* harmony export (binding) */ __webpack_require__.d(__webpack_exports__, "setOffsetToUTC", function() { return setOffsetToUTC; });
/* harmony export (binding) */ __webpack_require__.d(__webpack_exports__, "isDaylightSavingTime", function() { return isDaylightSavingTime; });
/* harmony export (binding) */ __webpack_require__.d(__webpack_exports__, "setOffsetToParsedOffset", function() { return setOffsetToParsedOffset; });
/* harmony export (binding) */ __webpack_require__.d(__webpack_exports__, "hasAlignedHourOffset", function() { return hasAlignedHourOffset; });
/* harmony import */ var _format_format__WEBPACK_IMPORTED_MODULE_0__ = __webpack_require__(/*! ../format/format */ "./node_modules/ngx-bootstrap/chronos/format/format.js");
/* harmony import */ var _utils_zero_fill__WEBPACK_IMPORTED_MODULE_1__ = __webpack_require__(/*! ../utils/zero-fill */ "./node_modules/ngx-bootstrap/chronos/utils/zero-fill.js");
/* harmony import */ var _utils_type_checks__WEBPACK_IMPORTED_MODULE_2__ = __webpack_require__(/*! ../utils/type-checks */ "./node_modules/ngx-bootstrap/chronos/utils/type-checks.js");
/* harmony import */ var _parse_regex__WEBPACK_IMPORTED_MODULE_3__ = __webpack_require__(/*! ../parse/regex */ "./node_modules/ngx-bootstrap/chronos/parse/regex.js");
/* harmony import */ var _moment_add_subtract__WEBPACK_IMPORTED_MODULE_4__ = __webpack_require__(/*! ../moment/add-subtract */ "./node_modules/ngx-bootstrap/chronos/moment/add-subtract.js");
/* harmony import */ var _parse_token__WEBPACK_IMPORTED_MODULE_5__ = __webpack_require__(/*! ../parse/token */ "./node_modules/ngx-bootstrap/chronos/parse/token.js");
/* harmony import */ var _create_clone__WEBPACK_IMPORTED_MODULE_6__ = __webpack_require__(/*! ../create/clone */ "./node_modules/ngx-bootstrap/chronos/create/clone.js");
/* harmony import */ var _utils_date_setters__WEBPACK_IMPORTED_MODULE_7__ = __webpack_require__(/*! ../utils/date-setters */ "./node_modules/ngx-bootstrap/chronos/utils/date-setters.js");
// tslint:disable:no-bitwise max-line-length
// FORMATTING








function addOffsetFormatToken(token, separator) {
    Object(_format_format__WEBPACK_IMPORTED_MODULE_0__["addFormatToken"])(token, null, null, function (date, config) {
        var offset = getUTCOffset(date, { _isUTC: config.isUTC, _offset: config.offset });
        var sign = '+';
        if (offset < 0) {
            offset = -offset;
            sign = '-';
        }
        return sign + Object(_utils_zero_fill__WEBPACK_IMPORTED_MODULE_1__["zeroFill"])(~~(offset / 60), 2) + separator + Object(_utils_zero_fill__WEBPACK_IMPORTED_MODULE_1__["zeroFill"])(~~(offset) % 60, 2);
    });
}
addOffsetFormatToken('Z', ':');
addOffsetFormatToken('ZZ', '');
// PARSING
Object(_parse_regex__WEBPACK_IMPORTED_MODULE_3__["addRegexToken"])('Z', _parse_regex__WEBPACK_IMPORTED_MODULE_3__["matchShortOffset"]);
Object(_parse_regex__WEBPACK_IMPORTED_MODULE_3__["addRegexToken"])('ZZ', _parse_regex__WEBPACK_IMPORTED_MODULE_3__["matchShortOffset"]);
Object(_parse_token__WEBPACK_IMPORTED_MODULE_5__["addParseToken"])(['Z', 'ZZ'], function (input, array, config) {
    config._useUTC = true;
    config._tzm = offsetFromString(_parse_regex__WEBPACK_IMPORTED_MODULE_3__["matchShortOffset"], input);
    return config;
});
// HELPERS
// timezone chunker
// '+10:00' > ['10',  '00']
// '-1530'  > ['-15', '30']
var chunkOffset = /([\+\-]|\d\d)/gi;
function offsetFromString(matcher, str) {
    var matches = (str || '').match(matcher);
    if (matches === null) {
        return null;
    }
    var chunk = matches[matches.length - 1];
    var parts = chunk.match(chunkOffset) || ['-', '0', '0'];
    var minutes = parseInt(parts[1], 10) * 60 + Object(_utils_type_checks__WEBPACK_IMPORTED_MODULE_2__["toInt"])(parts[2]);
    var _min = parts[0] === '+' ? minutes : -minutes;
    return minutes === 0 ? 0 : _min;
}
// Return a moment from input, that is local/utc/zone equivalent to model.
function cloneWithOffset(input, date, config) {
    if (config === void 0) { config = {}; }
    if (!config._isUTC) {
        return input;
    }
    var res = Object(_create_clone__WEBPACK_IMPORTED_MODULE_6__["cloneDate"])(date);
    // todo: input._d - res._d + ((res._offset || 0) - (input._offset || 0))*60000
    var offsetDiff = (config._offset || 0) * 60000;
    var diff = input.valueOf() - res.valueOf() + offsetDiff;
    // Use low-level api, because this fn is low-level api.
    res.setTime(res.valueOf() + diff);
    // todo: add timezone handling
    // hooks.updateOffset(res, false);
    return res;
}
function getDateOffset(date) {
    // On Firefox.24 Date#getTimezoneOffset returns a floating point.
    // https://github.com/moment/moment/pull/1871
    return -Math.round(date.getTimezoneOffset() / 15) * 15;
}
// HOOKS
// This function will be called whenever a moment is mutated.
// It is intended to keep the offset in sync with the timezone.
// todo: it's from moment timezones
// hooks.updateOffset = function () {
// };
// MOMENTS
// keepLocalTime = true means only change the timezone, without
// affecting the local hour. So 5:31:26 +0300 --[utcOffset(2, true)]-->
// 5:31:26 +0200 It is possible that 5:31:26 doesn't exist with offset
// +0200, so we adjust the time as needed, to be valid.
//
// Keeping the time actually adds/subtracts (one hour)
// from the actual represented time. That is why we call updateOffset
// a second time. In case it wants us to change the offset again
// _changeInProgress == true case, then we have to adjust, because
// there is no such time in the given timezone.
function getUTCOffset(date, config) {
    if (config === void 0) { config = {}; }
    var _offset = config._offset || 0;
    return config._isUTC ? _offset : getDateOffset(date);
}
function setUTCOffset(date, input, keepLocalTime, keepMinutes, config) {
    if (config === void 0) { config = {}; }
    var offset = config._offset || 0;
    var localAdjust;
    var _input = input;
    var _date = date;
    if (Object(_utils_type_checks__WEBPACK_IMPORTED_MODULE_2__["isString"])(_input)) {
        _input = offsetFromString(_parse_regex__WEBPACK_IMPORTED_MODULE_3__["matchShortOffset"], _input);
        if (_input === null) {
            return _date;
        }
    }
    else if (Object(_utils_type_checks__WEBPACK_IMPORTED_MODULE_2__["isNumber"])(_input) && Math.abs(_input) < 16 && !keepMinutes) {
        _input = _input * 60;
    }
    if (!config._isUTC && keepLocalTime) {
        localAdjust = getDateOffset(_date);
    }
    config._offset = _input;
    config._isUTC = true;
    if (localAdjust != null) {
        _date = Object(_moment_add_subtract__WEBPACK_IMPORTED_MODULE_4__["add"])(_date, localAdjust, 'minutes');
    }
    if (offset !== _input) {
        if (!keepLocalTime || config._changeInProgress) {
            _date = Object(_moment_add_subtract__WEBPACK_IMPORTED_MODULE_4__["add"])(_date, _input - offset, 'minutes', config._isUTC);
            // addSubtract(this, createDuration(_input - offset, 'm'), 1, false);
        }
        else if (!config._changeInProgress) {
            config._changeInProgress = true;
            // todo: add timezone handling
            // hooks.updateOffset(this, true);
            config._changeInProgress = null;
        }
    }
    return _date;
}
/*
export function getSetZone(input, keepLocalTime) {
  if (input != null) {
    if (typeof input !== 'string') {
      input = -input;
    }

    this.utcOffset(input, keepLocalTime);

    return this;
  } else {
    return -this.utcOffset();
  }
}
*/
function setOffsetToUTC(date, keepLocalTime) {
    return setUTCOffset(date, 0, keepLocalTime);
}
function isDaylightSavingTime(date) {
    return (getUTCOffset(date) > getUTCOffset(Object(_utils_date_setters__WEBPACK_IMPORTED_MODULE_7__["setMonth"])(Object(_create_clone__WEBPACK_IMPORTED_MODULE_6__["cloneDate"])(date), 0))
        || getUTCOffset(date) > getUTCOffset(Object(_utils_date_setters__WEBPACK_IMPORTED_MODULE_7__["setMonth"])(Object(_create_clone__WEBPACK_IMPORTED_MODULE_6__["cloneDate"])(date), 5)));
}
/*export function setOffsetToLocal(date: Date, isUTC?: boolean, keepLocalTime?: boolean) {
  if (this._isUTC) {
    this.utcOffset(0, keepLocalTime);
    this._isUTC = false;

    if (keepLocalTime) {
      this.subtract(getDateOffset(this), 'm');
    }
  }
  return this;
}*/
function setOffsetToParsedOffset(date, input, config) {
    if (config === void 0) { config = {}; }
    if (config._tzm != null) {
        return setUTCOffset(date, config._tzm, false, true, config);
    }
    if (Object(_utils_type_checks__WEBPACK_IMPORTED_MODULE_2__["isString"])(input)) {
        var tZone = offsetFromString(_parse_regex__WEBPACK_IMPORTED_MODULE_3__["matchOffset"], input);
        if (tZone != null) {
            return setUTCOffset(date, tZone, false, false, config);
        }
        return setUTCOffset(date, 0, true, false, config);
    }
    return date;
}
function hasAlignedHourOffset(date, input) {
    var _input = input ? getUTCOffset(input, { _isUTC: false }) : 0;
    return (getUTCOffset(date) - _input) % 60 === 0;
}
// DEPRECATED
/*export function isDaylightSavingTimeShifted() {
  if (!isUndefined(this._isDSTShifted)) {
    return this._isDSTShifted;
  }

  const c = {};

  copyConfig(c, this);
  c = prepareConfig(c);

  if (c._a) {
    const other = c._isUTC ? createUTC(c._a) : createLocal(c._a);
    this._isDSTShifted = this.isValid() &&
      compareArrays(c._a, other.toArray()) > 0;
  } else {
    this._isDSTShifted = false;
  }

  return this._isDSTShifted;
}*/
// in Khronos
/*export function isLocal() {
  return this.isValid() ? !this._isUTC : false;
}

export function isUtcOffset() {
  return this.isValid() ? this._isUTC : false;
}

export function isUtc() {
  return this.isValid() ? this._isUTC && this._offset === 0 : false;
}*/
//# sourceMappingURL=offset.js.map

/***/ }),

/***/ "./node_modules/ngx-bootstrap/chronos/units/priorities.js":
/*!****************************************************************!*\
  !*** ./node_modules/ngx-bootstrap/chronos/units/priorities.js ***!
  \****************************************************************/
/*! exports provided: addUnitPriority */
/***/ (function(module, __webpack_exports__, __webpack_require__) {

"use strict";
__webpack_require__.r(__webpack_exports__);
/* harmony export (binding) */ __webpack_require__.d(__webpack_exports__, "addUnitPriority", function() { return addUnitPriority; });
var priorities = {};
function addUnitPriority(unit, priority) {
    priorities[unit] = priority;
}
/*
export function getPrioritizedUnits(unitsObj) {
  const units = [];
  let unit;
  for (unit in unitsObj) {
    if (unitsObj.hasOwnProperty(unit)) {
      units.push({ unit, priority: priorities[unit] });
    }
  }
  units.sort(function (a, b) {
    return a.priority - b.priority;
  });

  return units;
}
*/
//# sourceMappingURL=priorities.js.map

/***/ }),

/***/ "./node_modules/ngx-bootstrap/chronos/units/quarter.js":
/*!*************************************************************!*\
  !*** ./node_modules/ngx-bootstrap/chronos/units/quarter.js ***!
  \*************************************************************/
/*! exports provided: getQuarter, setQuarter */
/***/ (function(module, __webpack_exports__, __webpack_require__) {

"use strict";
__webpack_require__.r(__webpack_exports__);
/* harmony export (binding) */ __webpack_require__.d(__webpack_exports__, "getQuarter", function() { return getQuarter; });
/* harmony export (binding) */ __webpack_require__.d(__webpack_exports__, "setQuarter", function() { return setQuarter; });
/* harmony import */ var _format_format__WEBPACK_IMPORTED_MODULE_0__ = __webpack_require__(/*! ../format/format */ "./node_modules/ngx-bootstrap/chronos/format/format.js");
/* harmony import */ var _parse_regex__WEBPACK_IMPORTED_MODULE_1__ = __webpack_require__(/*! ../parse/regex */ "./node_modules/ngx-bootstrap/chronos/parse/regex.js");
/* harmony import */ var _parse_token__WEBPACK_IMPORTED_MODULE_2__ = __webpack_require__(/*! ../parse/token */ "./node_modules/ngx-bootstrap/chronos/parse/token.js");
/* harmony import */ var _constants__WEBPACK_IMPORTED_MODULE_3__ = __webpack_require__(/*! ./constants */ "./node_modules/ngx-bootstrap/chronos/units/constants.js");
/* harmony import */ var _utils_type_checks__WEBPACK_IMPORTED_MODULE_4__ = __webpack_require__(/*! ../utils/type-checks */ "./node_modules/ngx-bootstrap/chronos/utils/type-checks.js");
/* harmony import */ var _utils_date_getters__WEBPACK_IMPORTED_MODULE_5__ = __webpack_require__(/*! ../utils/date-getters */ "./node_modules/ngx-bootstrap/chronos/utils/date-getters.js");
/* harmony import */ var _priorities__WEBPACK_IMPORTED_MODULE_6__ = __webpack_require__(/*! ./priorities */ "./node_modules/ngx-bootstrap/chronos/units/priorities.js");
/* harmony import */ var _aliases__WEBPACK_IMPORTED_MODULE_7__ = __webpack_require__(/*! ./aliases */ "./node_modules/ngx-bootstrap/chronos/units/aliases.js");
/* harmony import */ var _utils_date_setters__WEBPACK_IMPORTED_MODULE_8__ = __webpack_require__(/*! ../utils/date-setters */ "./node_modules/ngx-bootstrap/chronos/utils/date-setters.js");









// FORMATTING
Object(_format_format__WEBPACK_IMPORTED_MODULE_0__["addFormatToken"])('Q', null, 'Qo', function (date, opts) {
    return getQuarter(date, opts.isUTC).toString(10);
});
// ALIASES
Object(_aliases__WEBPACK_IMPORTED_MODULE_7__["addUnitAlias"])('quarter', 'Q');
// PRIORITY
Object(_priorities__WEBPACK_IMPORTED_MODULE_6__["addUnitPriority"])('quarter', 7);
// PARSING
Object(_parse_regex__WEBPACK_IMPORTED_MODULE_1__["addRegexToken"])('Q', _parse_regex__WEBPACK_IMPORTED_MODULE_1__["match1"]);
Object(_parse_token__WEBPACK_IMPORTED_MODULE_2__["addParseToken"])('Q', function (input, array, config) {
    array[_constants__WEBPACK_IMPORTED_MODULE_3__["MONTH"]] = (Object(_utils_type_checks__WEBPACK_IMPORTED_MODULE_4__["toInt"])(input) - 1) * 3;
    return config;
});
// MOMENTS
function getQuarter(date, isUTC) {
    if (isUTC === void 0) { isUTC = false; }
    return Math.ceil((Object(_utils_date_getters__WEBPACK_IMPORTED_MODULE_5__["getMonth"])(date, isUTC) + 1) / 3);
}
function setQuarter(date, quarter, isUTC) {
    return Object(_utils_date_setters__WEBPACK_IMPORTED_MODULE_8__["setMonth"])(date, (quarter - 1) * 3 + Object(_utils_date_getters__WEBPACK_IMPORTED_MODULE_5__["getMonth"])(date, isUTC) % 3, isUTC);
}
// export function getSetQuarter(input) {
//   return input == null
//     ? Math.ceil((this.month() + 1) / 3)
//     : this.month((input - 1) * 3 + this.month() % 3);
// }
//# sourceMappingURL=quarter.js.map

/***/ }),

/***/ "./node_modules/ngx-bootstrap/chronos/units/second.js":
/*!************************************************************!*\
  !*** ./node_modules/ngx-bootstrap/chronos/units/second.js ***!
  \************************************************************/
/*! no exports provided */
/***/ (function(module, __webpack_exports__, __webpack_require__) {

"use strict";
__webpack_require__.r(__webpack_exports__);
/* harmony import */ var _format_format__WEBPACK_IMPORTED_MODULE_0__ = __webpack_require__(/*! ../format/format */ "./node_modules/ngx-bootstrap/chronos/format/format.js");
/* harmony import */ var _utils_date_getters__WEBPACK_IMPORTED_MODULE_1__ = __webpack_require__(/*! ../utils/date-getters */ "./node_modules/ngx-bootstrap/chronos/utils/date-getters.js");
/* harmony import */ var _parse_regex__WEBPACK_IMPORTED_MODULE_2__ = __webpack_require__(/*! ../parse/regex */ "./node_modules/ngx-bootstrap/chronos/parse/regex.js");
/* harmony import */ var _parse_token__WEBPACK_IMPORTED_MODULE_3__ = __webpack_require__(/*! ../parse/token */ "./node_modules/ngx-bootstrap/chronos/parse/token.js");
/* harmony import */ var _constants__WEBPACK_IMPORTED_MODULE_4__ = __webpack_require__(/*! ./constants */ "./node_modules/ngx-bootstrap/chronos/units/constants.js");
/* harmony import */ var _aliases__WEBPACK_IMPORTED_MODULE_5__ = __webpack_require__(/*! ./aliases */ "./node_modules/ngx-bootstrap/chronos/units/aliases.js");
/* harmony import */ var _priorities__WEBPACK_IMPORTED_MODULE_6__ = __webpack_require__(/*! ./priorities */ "./node_modules/ngx-bootstrap/chronos/units/priorities.js");







// FORMATTING
Object(_format_format__WEBPACK_IMPORTED_MODULE_0__["addFormatToken"])('s', ['ss', 2, false], null, function (date, opts) {
    return Object(_utils_date_getters__WEBPACK_IMPORTED_MODULE_1__["getSeconds"])(date, opts.isUTC).toString(10);
});
// ALIASES
Object(_aliases__WEBPACK_IMPORTED_MODULE_5__["addUnitAlias"])('second', 's');
// PRIORITY
Object(_priorities__WEBPACK_IMPORTED_MODULE_6__["addUnitPriority"])('second', 15);
// PARSING
Object(_parse_regex__WEBPACK_IMPORTED_MODULE_2__["addRegexToken"])('s', _parse_regex__WEBPACK_IMPORTED_MODULE_2__["match1to2"]);
Object(_parse_regex__WEBPACK_IMPORTED_MODULE_2__["addRegexToken"])('ss', _parse_regex__WEBPACK_IMPORTED_MODULE_2__["match1to2"], _parse_regex__WEBPACK_IMPORTED_MODULE_2__["match2"]);
Object(_parse_token__WEBPACK_IMPORTED_MODULE_3__["addParseToken"])(['s', 'ss'], _constants__WEBPACK_IMPORTED_MODULE_4__["SECOND"]);
//# sourceMappingURL=second.js.map

/***/ }),

/***/ "./node_modules/ngx-bootstrap/chronos/units/timestamp.js":
/*!***************************************************************!*\
  !*** ./node_modules/ngx-bootstrap/chronos/units/timestamp.js ***!
  \***************************************************************/
/*! no exports provided */
/***/ (function(module, __webpack_exports__, __webpack_require__) {

"use strict";
__webpack_require__.r(__webpack_exports__);
/* harmony import */ var _format_format__WEBPACK_IMPORTED_MODULE_0__ = __webpack_require__(/*! ../format/format */ "./node_modules/ngx-bootstrap/chronos/format/format.js");
/* harmony import */ var _utils_date_getters__WEBPACK_IMPORTED_MODULE_1__ = __webpack_require__(/*! ../utils/date-getters */ "./node_modules/ngx-bootstrap/chronos/utils/date-getters.js");
/* harmony import */ var _parse_regex__WEBPACK_IMPORTED_MODULE_2__ = __webpack_require__(/*! ../parse/regex */ "./node_modules/ngx-bootstrap/chronos/parse/regex.js");
/* harmony import */ var _parse_token__WEBPACK_IMPORTED_MODULE_3__ = __webpack_require__(/*! ../parse/token */ "./node_modules/ngx-bootstrap/chronos/parse/token.js");
/* harmony import */ var _utils_type_checks__WEBPACK_IMPORTED_MODULE_4__ = __webpack_require__(/*! ../utils/type-checks */ "./node_modules/ngx-bootstrap/chronos/utils/type-checks.js");





// FORMATTING
Object(_format_format__WEBPACK_IMPORTED_MODULE_0__["addFormatToken"])('X', null, null, function (date) {
    return Object(_utils_date_getters__WEBPACK_IMPORTED_MODULE_1__["unix"])(date).toString(10);
});
Object(_format_format__WEBPACK_IMPORTED_MODULE_0__["addFormatToken"])('x', null, null, function (date) {
    return date.valueOf().toString(10);
});
// PARSING
Object(_parse_regex__WEBPACK_IMPORTED_MODULE_2__["addRegexToken"])('x', _parse_regex__WEBPACK_IMPORTED_MODULE_2__["matchSigned"]);
Object(_parse_regex__WEBPACK_IMPORTED_MODULE_2__["addRegexToken"])('X', _parse_regex__WEBPACK_IMPORTED_MODULE_2__["matchTimestamp"]);
Object(_parse_token__WEBPACK_IMPORTED_MODULE_3__["addParseToken"])('X', function (input, array, config) {
    config._d = new Date(parseFloat(input) * 1000);
    return config;
});
Object(_parse_token__WEBPACK_IMPORTED_MODULE_3__["addParseToken"])('x', function (input, array, config) {
    config._d = new Date(Object(_utils_type_checks__WEBPACK_IMPORTED_MODULE_4__["toInt"])(input));
    return config;
});
//# sourceMappingURL=timestamp.js.map

/***/ }),

/***/ "./node_modules/ngx-bootstrap/chronos/units/timezone.js":
/*!**************************************************************!*\
  !*** ./node_modules/ngx-bootstrap/chronos/units/timezone.js ***!
  \**************************************************************/
/*! exports provided: getZoneAbbr, getZoneName */
/***/ (function(module, __webpack_exports__, __webpack_require__) {

"use strict";
__webpack_require__.r(__webpack_exports__);
/* harmony export (binding) */ __webpack_require__.d(__webpack_exports__, "getZoneAbbr", function() { return getZoneAbbr; });
/* harmony export (binding) */ __webpack_require__.d(__webpack_exports__, "getZoneName", function() { return getZoneName; });
/* harmony import */ var _format_format__WEBPACK_IMPORTED_MODULE_0__ = __webpack_require__(/*! ../format/format */ "./node_modules/ngx-bootstrap/chronos/format/format.js");

// todo: add support for timezones
// FORMATTING
Object(_format_format__WEBPACK_IMPORTED_MODULE_0__["addFormatToken"])('z', null, null, function (date, opts) {
    return opts.isUTC ? 'UTC' : '';
});
Object(_format_format__WEBPACK_IMPORTED_MODULE_0__["addFormatToken"])('zz', null, null, function (date, opts) {
    return opts.isUTC ? 'Coordinated Universal Time' : '';
});
// MOMENTS
function getZoneAbbr(isUTC) {
    return isUTC ? 'UTC' : '';
}
function getZoneName(isUTC) {
    return isUTC ? 'Coordinated Universal Time' : '';
}
//# sourceMappingURL=timezone.js.map

/***/ }),

/***/ "./node_modules/ngx-bootstrap/chronos/units/week-calendar-utils.js":
/*!*************************************************************************!*\
  !*** ./node_modules/ngx-bootstrap/chronos/units/week-calendar-utils.js ***!
  \*************************************************************************/
/*! exports provided: dayOfYearFromWeeks, weekOfYear, weeksInYear */
/***/ (function(module, __webpack_exports__, __webpack_require__) {

"use strict";
__webpack_require__.r(__webpack_exports__);
/* harmony export (binding) */ __webpack_require__.d(__webpack_exports__, "dayOfYearFromWeeks", function() { return dayOfYearFromWeeks; });
/* harmony export (binding) */ __webpack_require__.d(__webpack_exports__, "weekOfYear", function() { return weekOfYear; });
/* harmony export (binding) */ __webpack_require__.d(__webpack_exports__, "weeksInYear", function() { return weeksInYear; });
/* harmony import */ var _create_date_from_array__WEBPACK_IMPORTED_MODULE_0__ = __webpack_require__(/*! ../create/date-from-array */ "./node_modules/ngx-bootstrap/chronos/create/date-from-array.js");
/* harmony import */ var _year__WEBPACK_IMPORTED_MODULE_1__ = __webpack_require__(/*! ./year */ "./node_modules/ngx-bootstrap/chronos/units/year.js");
/* harmony import */ var _day_of_year__WEBPACK_IMPORTED_MODULE_2__ = __webpack_require__(/*! ./day-of-year */ "./node_modules/ngx-bootstrap/chronos/units/day-of-year.js");
/* harmony import */ var _utils_date_getters__WEBPACK_IMPORTED_MODULE_3__ = __webpack_require__(/*! ../utils/date-getters */ "./node_modules/ngx-bootstrap/chronos/utils/date-getters.js");




function firstWeekOffset(year, dow, doy) {
    // first-week day -- which january is always in the first week (4 for iso, 1 for other)
    var fwd = dow - doy + 7;
    // first-week day local weekday -- which local weekday is fwd
    var fwdlw = (Object(_create_date_from_array__WEBPACK_IMPORTED_MODULE_0__["createUTCDate"])(year, 0, fwd).getUTCDay() - dow + 7) % 7;
    return -fwdlw + fwd - 1;
}
// https://en.wikipedia.org/wiki/ISO_week_date#Calculating_a_date_given_the_year.2C_week_number_and_weekday
function dayOfYearFromWeeks(year, week, weekday, dow, doy) {
    var localWeekday = (7 + weekday - dow) % 7;
    var weekOffset = firstWeekOffset(year, dow, doy);
    var dayOfYear = 1 + 7 * (week - 1) + localWeekday + weekOffset;
    var resYear;
    var resDayOfYear;
    if (dayOfYear <= 0) {
        resYear = year - 1;
        resDayOfYear = Object(_year__WEBPACK_IMPORTED_MODULE_1__["daysInYear"])(resYear) + dayOfYear;
    }
    else if (dayOfYear > Object(_year__WEBPACK_IMPORTED_MODULE_1__["daysInYear"])(year)) {
        resYear = year + 1;
        resDayOfYear = dayOfYear - Object(_year__WEBPACK_IMPORTED_MODULE_1__["daysInYear"])(year);
    }
    else {
        resYear = year;
        resDayOfYear = dayOfYear;
    }
    return {
        year: resYear,
        dayOfYear: resDayOfYear
    };
}
function weekOfYear(date, dow, doy, isUTC) {
    var weekOffset = firstWeekOffset(Object(_utils_date_getters__WEBPACK_IMPORTED_MODULE_3__["getFullYear"])(date, isUTC), dow, doy);
    var week = Math.floor((Object(_day_of_year__WEBPACK_IMPORTED_MODULE_2__["getDayOfYear"])(date, isUTC) - weekOffset - 1) / 7) + 1;
    var resWeek;
    var resYear;
    if (week < 1) {
        resYear = Object(_utils_date_getters__WEBPACK_IMPORTED_MODULE_3__["getFullYear"])(date, isUTC) - 1;
        resWeek = week + weeksInYear(resYear, dow, doy);
    }
    else if (week > weeksInYear(Object(_utils_date_getters__WEBPACK_IMPORTED_MODULE_3__["getFullYear"])(date, isUTC), dow, doy)) {
        resWeek = week - weeksInYear(Object(_utils_date_getters__WEBPACK_IMPORTED_MODULE_3__["getFullYear"])(date, isUTC), dow, doy);
        resYear = Object(_utils_date_getters__WEBPACK_IMPORTED_MODULE_3__["getFullYear"])(date, isUTC) + 1;
    }
    else {
        resYear = Object(_utils_date_getters__WEBPACK_IMPORTED_MODULE_3__["getFullYear"])(date, isUTC);
        resWeek = week;
    }
    return {
        week: resWeek,
        year: resYear
    };
}
function weeksInYear(year, dow, doy) {
    var weekOffset = firstWeekOffset(year, dow, doy);
    var weekOffsetNext = firstWeekOffset(year + 1, dow, doy);
    return (Object(_year__WEBPACK_IMPORTED_MODULE_1__["daysInYear"])(year) - weekOffset + weekOffsetNext) / 7;
}
//# sourceMappingURL=week-calendar-utils.js.map

/***/ }),

/***/ "./node_modules/ngx-bootstrap/chronos/units/week-year.js":
/*!***************************************************************!*\
  !*** ./node_modules/ngx-bootstrap/chronos/units/week-year.js ***!
  \***************************************************************/
/*! exports provided: getSetWeekYear, getWeekYear, getSetISOWeekYear, getISOWeekYear, getISOWeeksInYear, getWeeksInYear */
/***/ (function(module, __webpack_exports__, __webpack_require__) {

"use strict";
__webpack_require__.r(__webpack_exports__);
/* harmony export (binding) */ __webpack_require__.d(__webpack_exports__, "getSetWeekYear", function() { return getSetWeekYear; });
/* harmony export (binding) */ __webpack_require__.d(__webpack_exports__, "getWeekYear", function() { return getWeekYear; });
/* harmony export (binding) */ __webpack_require__.d(__webpack_exports__, "getSetISOWeekYear", function() { return getSetISOWeekYear; });
/* harmony export (binding) */ __webpack_require__.d(__webpack_exports__, "getISOWeekYear", function() { return getISOWeekYear; });
/* harmony export (binding) */ __webpack_require__.d(__webpack_exports__, "getISOWeeksInYear", function() { return getISOWeeksInYear; });
/* harmony export (binding) */ __webpack_require__.d(__webpack_exports__, "getWeeksInYear", function() { return getWeeksInYear; });
/* harmony import */ var _format_format__WEBPACK_IMPORTED_MODULE_0__ = __webpack_require__(/*! ../format/format */ "./node_modules/ngx-bootstrap/chronos/format/format.js");
/* harmony import */ var _aliases__WEBPACK_IMPORTED_MODULE_1__ = __webpack_require__(/*! ./aliases */ "./node_modules/ngx-bootstrap/chronos/units/aliases.js");
/* harmony import */ var _priorities__WEBPACK_IMPORTED_MODULE_2__ = __webpack_require__(/*! ./priorities */ "./node_modules/ngx-bootstrap/chronos/units/priorities.js");
/* harmony import */ var _parse_regex__WEBPACK_IMPORTED_MODULE_3__ = __webpack_require__(/*! ../parse/regex */ "./node_modules/ngx-bootstrap/chronos/parse/regex.js");
/* harmony import */ var _parse_token__WEBPACK_IMPORTED_MODULE_4__ = __webpack_require__(/*! ../parse/token */ "./node_modules/ngx-bootstrap/chronos/parse/token.js");
/* harmony import */ var _utils_type_checks__WEBPACK_IMPORTED_MODULE_5__ = __webpack_require__(/*! ../utils/type-checks */ "./node_modules/ngx-bootstrap/chronos/utils/type-checks.js");
/* harmony import */ var _year__WEBPACK_IMPORTED_MODULE_6__ = __webpack_require__(/*! ./year */ "./node_modules/ngx-bootstrap/chronos/units/year.js");
/* harmony import */ var _week_calendar_utils__WEBPACK_IMPORTED_MODULE_7__ = __webpack_require__(/*! ./week-calendar-utils */ "./node_modules/ngx-bootstrap/chronos/units/week-calendar-utils.js");
/* harmony import */ var _create_date_from_array__WEBPACK_IMPORTED_MODULE_8__ = __webpack_require__(/*! ../create/date-from-array */ "./node_modules/ngx-bootstrap/chronos/create/date-from-array.js");
/* harmony import */ var _week__WEBPACK_IMPORTED_MODULE_9__ = __webpack_require__(/*! ./week */ "./node_modules/ngx-bootstrap/chronos/units/week.js");
/* harmony import */ var _day_of_week__WEBPACK_IMPORTED_MODULE_10__ = __webpack_require__(/*! ./day-of-week */ "./node_modules/ngx-bootstrap/chronos/units/day-of-week.js");
/* harmony import */ var _locale_locales__WEBPACK_IMPORTED_MODULE_11__ = __webpack_require__(/*! ../locale/locales */ "./node_modules/ngx-bootstrap/chronos/locale/locales.js");
/* harmony import */ var _utils_date_setters__WEBPACK_IMPORTED_MODULE_12__ = __webpack_require__(/*! ../utils/date-setters */ "./node_modules/ngx-bootstrap/chronos/utils/date-setters.js");
/* harmony import */ var _utils_date_getters__WEBPACK_IMPORTED_MODULE_13__ = __webpack_require__(/*! ../utils/date-getters */ "./node_modules/ngx-bootstrap/chronos/utils/date-getters.js");














// FORMATTING
Object(_format_format__WEBPACK_IMPORTED_MODULE_0__["addFormatToken"])(null, ['gg', 2, false], null, function (date, opts) {
    // return this.weekYear() % 100;
    return (getWeekYear(date, opts.locale) % 100).toString();
});
Object(_format_format__WEBPACK_IMPORTED_MODULE_0__["addFormatToken"])(null, ['GG', 2, false], null, function (date) {
    // return this.isoWeekYear() % 100;
    return (getISOWeekYear(date) % 100).toString();
});
function addWeekYearFormatToken(token, getter) {
    Object(_format_format__WEBPACK_IMPORTED_MODULE_0__["addFormatToken"])(null, [token, token.length, false], null, getter);
}
function _getWeekYearFormatCb(date, opts) {
    return getWeekYear(date, opts.locale).toString();
}
function _getISOWeekYearFormatCb(date) {
    return getISOWeekYear(date).toString();
}
addWeekYearFormatToken('gggg', _getWeekYearFormatCb);
addWeekYearFormatToken('ggggg', _getWeekYearFormatCb);
addWeekYearFormatToken('GGGG', _getISOWeekYearFormatCb);
addWeekYearFormatToken('GGGGG', _getISOWeekYearFormatCb);
// ALIASES
Object(_aliases__WEBPACK_IMPORTED_MODULE_1__["addUnitAlias"])('weekYear', 'gg');
Object(_aliases__WEBPACK_IMPORTED_MODULE_1__["addUnitAlias"])('isoWeekYear', 'GG');
// PRIORITY
Object(_priorities__WEBPACK_IMPORTED_MODULE_2__["addUnitPriority"])('weekYear', 1);
Object(_priorities__WEBPACK_IMPORTED_MODULE_2__["addUnitPriority"])('isoWeekYear', 1);
// PARSING
Object(_parse_regex__WEBPACK_IMPORTED_MODULE_3__["addRegexToken"])('G', _parse_regex__WEBPACK_IMPORTED_MODULE_3__["matchSigned"]);
Object(_parse_regex__WEBPACK_IMPORTED_MODULE_3__["addRegexToken"])('g', _parse_regex__WEBPACK_IMPORTED_MODULE_3__["matchSigned"]);
Object(_parse_regex__WEBPACK_IMPORTED_MODULE_3__["addRegexToken"])('GG', _parse_regex__WEBPACK_IMPORTED_MODULE_3__["match1to2"], _parse_regex__WEBPACK_IMPORTED_MODULE_3__["match2"]);
Object(_parse_regex__WEBPACK_IMPORTED_MODULE_3__["addRegexToken"])('gg', _parse_regex__WEBPACK_IMPORTED_MODULE_3__["match1to2"], _parse_regex__WEBPACK_IMPORTED_MODULE_3__["match2"]);
Object(_parse_regex__WEBPACK_IMPORTED_MODULE_3__["addRegexToken"])('GGGG', _parse_regex__WEBPACK_IMPORTED_MODULE_3__["match1to4"], _parse_regex__WEBPACK_IMPORTED_MODULE_3__["match4"]);
Object(_parse_regex__WEBPACK_IMPORTED_MODULE_3__["addRegexToken"])('gggg', _parse_regex__WEBPACK_IMPORTED_MODULE_3__["match1to4"], _parse_regex__WEBPACK_IMPORTED_MODULE_3__["match4"]);
Object(_parse_regex__WEBPACK_IMPORTED_MODULE_3__["addRegexToken"])('GGGGG', _parse_regex__WEBPACK_IMPORTED_MODULE_3__["match1to6"], _parse_regex__WEBPACK_IMPORTED_MODULE_3__["match6"]);
Object(_parse_regex__WEBPACK_IMPORTED_MODULE_3__["addRegexToken"])('ggggg', _parse_regex__WEBPACK_IMPORTED_MODULE_3__["match1to6"], _parse_regex__WEBPACK_IMPORTED_MODULE_3__["match6"]);
Object(_parse_token__WEBPACK_IMPORTED_MODULE_4__["addWeekParseToken"])(['gggg', 'ggggg', 'GGGG', 'GGGGG'], function (input, week, config, token) {
    week[token.substr(0, 2)] = Object(_utils_type_checks__WEBPACK_IMPORTED_MODULE_5__["toInt"])(input);
    return config;
});
Object(_parse_token__WEBPACK_IMPORTED_MODULE_4__["addWeekParseToken"])(['gg', 'GG'], function (input, week, config, token) {
    week[token] = Object(_year__WEBPACK_IMPORTED_MODULE_6__["parseTwoDigitYear"])(input);
    return config;
});
// MOMENTS
function getSetWeekYear(date, input, locale, isUTC) {
    if (locale === void 0) { locale = Object(_locale_locales__WEBPACK_IMPORTED_MODULE_11__["getLocale"])(); }
    return getSetWeekYearHelper(date, input, 
    // this.week(),
    Object(_week__WEBPACK_IMPORTED_MODULE_9__["getWeek"])(date, locale, isUTC), 
    // this.weekday(),
    Object(_day_of_week__WEBPACK_IMPORTED_MODULE_10__["getLocaleDayOfWeek"])(date, locale, isUTC), locale.firstDayOfWeek(), locale.firstDayOfYear(), isUTC);
}
function getWeekYear(date, locale, isUTC) {
    if (locale === void 0) { locale = Object(_locale_locales__WEBPACK_IMPORTED_MODULE_11__["getLocale"])(); }
    return Object(_week_calendar_utils__WEBPACK_IMPORTED_MODULE_7__["weekOfYear"])(date, locale.firstDayOfWeek(), locale.firstDayOfYear(), isUTC).year;
}
function getSetISOWeekYear(date, input, isUTC) {
    return getSetWeekYearHelper(date, input, Object(_week__WEBPACK_IMPORTED_MODULE_9__["getISOWeek"])(date, isUTC), Object(_day_of_week__WEBPACK_IMPORTED_MODULE_10__["getISODayOfWeek"])(date, isUTC), 1, 4);
}
function getISOWeekYear(date, isUTC) {
    return Object(_week_calendar_utils__WEBPACK_IMPORTED_MODULE_7__["weekOfYear"])(date, 1, 4, isUTC).year;
}
function getISOWeeksInYear(date, isUTC) {
    return Object(_week_calendar_utils__WEBPACK_IMPORTED_MODULE_7__["weeksInYear"])(Object(_utils_date_getters__WEBPACK_IMPORTED_MODULE_13__["getFullYear"])(date, isUTC), 1, 4);
}
function getWeeksInYear(date, isUTC, locale) {
    if (locale === void 0) { locale = Object(_locale_locales__WEBPACK_IMPORTED_MODULE_11__["getLocale"])(); }
    return Object(_week_calendar_utils__WEBPACK_IMPORTED_MODULE_7__["weeksInYear"])(Object(_utils_date_getters__WEBPACK_IMPORTED_MODULE_13__["getFullYear"])(date, isUTC), locale.firstDayOfWeek(), locale.firstDayOfYear());
}
function getSetWeekYearHelper(date, input, week, weekday, dow, doy, isUTC) {
    if (!input) {
        return getWeekYear(date, void 0, isUTC);
    }
    var weeksTarget = Object(_week_calendar_utils__WEBPACK_IMPORTED_MODULE_7__["weeksInYear"])(input, dow, doy);
    var _week = week > weeksTarget ? weeksTarget : week;
    return setWeekAll(date, input, _week, weekday, dow, doy);
}
function setWeekAll(date, weekYear, week, weekday, dow, doy) {
    var dayOfYearData = Object(_week_calendar_utils__WEBPACK_IMPORTED_MODULE_7__["dayOfYearFromWeeks"])(weekYear, week, weekday, dow, doy);
    var _date = Object(_create_date_from_array__WEBPACK_IMPORTED_MODULE_8__["createUTCDate"])(dayOfYearData.year, 0, dayOfYearData.dayOfYear);
    Object(_utils_date_setters__WEBPACK_IMPORTED_MODULE_12__["setFullYear"])(date, Object(_utils_date_getters__WEBPACK_IMPORTED_MODULE_13__["getFullYear"])(_date, true), true);
    Object(_utils_date_setters__WEBPACK_IMPORTED_MODULE_12__["setMonth"])(date, Object(_utils_date_getters__WEBPACK_IMPORTED_MODULE_13__["getMonth"])(_date, true), true);
    Object(_utils_date_setters__WEBPACK_IMPORTED_MODULE_12__["setDate"])(date, Object(_utils_date_getters__WEBPACK_IMPORTED_MODULE_13__["getDate"])(_date, true), true);
    return date;
}
//# sourceMappingURL=week-year.js.map

/***/ }),

/***/ "./node_modules/ngx-bootstrap/chronos/units/week.js":
/*!**********************************************************!*\
  !*** ./node_modules/ngx-bootstrap/chronos/units/week.js ***!
  \**********************************************************/
/*! exports provided: setWeek, getWeek, setISOWeek, getISOWeek */
/***/ (function(module, __webpack_exports__, __webpack_require__) {

"use strict";
__webpack_require__.r(__webpack_exports__);
/* harmony export (binding) */ __webpack_require__.d(__webpack_exports__, "setWeek", function() { return setWeek; });
/* harmony export (binding) */ __webpack_require__.d(__webpack_exports__, "getWeek", function() { return getWeek; });
/* harmony export (binding) */ __webpack_require__.d(__webpack_exports__, "setISOWeek", function() { return setISOWeek; });
/* harmony export (binding) */ __webpack_require__.d(__webpack_exports__, "getISOWeek", function() { return getISOWeek; });
/* harmony import */ var _format_format__WEBPACK_IMPORTED_MODULE_0__ = __webpack_require__(/*! ../format/format */ "./node_modules/ngx-bootstrap/chronos/format/format.js");
/* harmony import */ var _week_calendar_utils__WEBPACK_IMPORTED_MODULE_1__ = __webpack_require__(/*! ./week-calendar-utils */ "./node_modules/ngx-bootstrap/chronos/units/week-calendar-utils.js");
/* harmony import */ var _parse_regex__WEBPACK_IMPORTED_MODULE_2__ = __webpack_require__(/*! ../parse/regex */ "./node_modules/ngx-bootstrap/chronos/parse/regex.js");
/* harmony import */ var _aliases__WEBPACK_IMPORTED_MODULE_3__ = __webpack_require__(/*! ./aliases */ "./node_modules/ngx-bootstrap/chronos/units/aliases.js");
/* harmony import */ var _priorities__WEBPACK_IMPORTED_MODULE_4__ = __webpack_require__(/*! ./priorities */ "./node_modules/ngx-bootstrap/chronos/units/priorities.js");
/* harmony import */ var _parse_token__WEBPACK_IMPORTED_MODULE_5__ = __webpack_require__(/*! ../parse/token */ "./node_modules/ngx-bootstrap/chronos/parse/token.js");
/* harmony import */ var _utils_type_checks__WEBPACK_IMPORTED_MODULE_6__ = __webpack_require__(/*! ../utils/type-checks */ "./node_modules/ngx-bootstrap/chronos/utils/type-checks.js");
/* harmony import */ var _locale_locales__WEBPACK_IMPORTED_MODULE_7__ = __webpack_require__(/*! ../locale/locales */ "./node_modules/ngx-bootstrap/chronos/locale/locales.js");
/* harmony import */ var _moment_add_subtract__WEBPACK_IMPORTED_MODULE_8__ = __webpack_require__(/*! ../moment/add-subtract */ "./node_modules/ngx-bootstrap/chronos/moment/add-subtract.js");









// FORMATTING
Object(_format_format__WEBPACK_IMPORTED_MODULE_0__["addFormatToken"])('w', ['ww', 2, false], 'wo', function (date, opts) {
    return getWeek(date, opts.locale).toString(10);
});
Object(_format_format__WEBPACK_IMPORTED_MODULE_0__["addFormatToken"])('W', ['WW', 2, false], 'Wo', function (date) {
    return getISOWeek(date).toString(10);
});
// ALIASES
Object(_aliases__WEBPACK_IMPORTED_MODULE_3__["addUnitAlias"])('week', 'w');
Object(_aliases__WEBPACK_IMPORTED_MODULE_3__["addUnitAlias"])('isoWeek', 'W');
// PRIORITIES
Object(_priorities__WEBPACK_IMPORTED_MODULE_4__["addUnitPriority"])('week', 5);
Object(_priorities__WEBPACK_IMPORTED_MODULE_4__["addUnitPriority"])('isoWeek', 5);
// PARSING
Object(_parse_regex__WEBPACK_IMPORTED_MODULE_2__["addRegexToken"])('w', _parse_regex__WEBPACK_IMPORTED_MODULE_2__["match1to2"]);
Object(_parse_regex__WEBPACK_IMPORTED_MODULE_2__["addRegexToken"])('ww', _parse_regex__WEBPACK_IMPORTED_MODULE_2__["match1to2"], _parse_regex__WEBPACK_IMPORTED_MODULE_2__["match2"]);
Object(_parse_regex__WEBPACK_IMPORTED_MODULE_2__["addRegexToken"])('W', _parse_regex__WEBPACK_IMPORTED_MODULE_2__["match1to2"]);
Object(_parse_regex__WEBPACK_IMPORTED_MODULE_2__["addRegexToken"])('WW', _parse_regex__WEBPACK_IMPORTED_MODULE_2__["match1to2"], _parse_regex__WEBPACK_IMPORTED_MODULE_2__["match2"]);
Object(_parse_token__WEBPACK_IMPORTED_MODULE_5__["addWeekParseToken"])(['w', 'ww', 'W', 'WW'], function (input, week, config, token) {
    week[token.substr(0, 1)] = Object(_utils_type_checks__WEBPACK_IMPORTED_MODULE_6__["toInt"])(input);
    return config;
});
// export function getSetWeek (input) {
//   var week = this.localeData().week(this);
//   return input == null ? week : this.add((input - week) * 7, 'd');
// }
function setWeek(date, input, locale) {
    if (locale === void 0) { locale = Object(_locale_locales__WEBPACK_IMPORTED_MODULE_7__["getLocale"])(); }
    var week = getWeek(date, locale);
    return Object(_moment_add_subtract__WEBPACK_IMPORTED_MODULE_8__["add"])(date, (input - week) * 7, 'day');
}
function getWeek(date, locale, isUTC) {
    if (locale === void 0) { locale = Object(_locale_locales__WEBPACK_IMPORTED_MODULE_7__["getLocale"])(); }
    return locale.week(date, isUTC);
}
// export function getSetISOWeek (input) {
//   var week = weekOfYear(this, 1, 4).week;
//   return input == null ? week : this.add((input - week) * 7, 'd');
// }
function setISOWeek(date, input) {
    var week = getISOWeek(date);
    return Object(_moment_add_subtract__WEBPACK_IMPORTED_MODULE_8__["add"])(date, (input - week) * 7, 'day');
}
function getISOWeek(date, isUTC) {
    return Object(_week_calendar_utils__WEBPACK_IMPORTED_MODULE_1__["weekOfYear"])(date, 1, 4, isUTC).week;
}
//# sourceMappingURL=week.js.map

/***/ }),

/***/ "./node_modules/ngx-bootstrap/chronos/units/year.js":
/*!**********************************************************!*\
  !*** ./node_modules/ngx-bootstrap/chronos/units/year.js ***!
  \**********************************************************/
/*! exports provided: parseTwoDigitYear, daysInYear, isLeapYear */
/***/ (function(module, __webpack_exports__, __webpack_require__) {

"use strict";
__webpack_require__.r(__webpack_exports__);
/* harmony export (binding) */ __webpack_require__.d(__webpack_exports__, "parseTwoDigitYear", function() { return parseTwoDigitYear; });
/* harmony export (binding) */ __webpack_require__.d(__webpack_exports__, "daysInYear", function() { return daysInYear; });
/* harmony export (binding) */ __webpack_require__.d(__webpack_exports__, "isLeapYear", function() { return isLeapYear; });
/* harmony import */ var _format_format__WEBPACK_IMPORTED_MODULE_0__ = __webpack_require__(/*! ../format/format */ "./node_modules/ngx-bootstrap/chronos/format/format.js");
/* harmony import */ var _utils_date_getters__WEBPACK_IMPORTED_MODULE_1__ = __webpack_require__(/*! ../utils/date-getters */ "./node_modules/ngx-bootstrap/chronos/utils/date-getters.js");
/* harmony import */ var _parse_regex__WEBPACK_IMPORTED_MODULE_2__ = __webpack_require__(/*! ../parse/regex */ "./node_modules/ngx-bootstrap/chronos/parse/regex.js");
/* harmony import */ var _parse_token__WEBPACK_IMPORTED_MODULE_3__ = __webpack_require__(/*! ../parse/token */ "./node_modules/ngx-bootstrap/chronos/parse/token.js");
/* harmony import */ var _constants__WEBPACK_IMPORTED_MODULE_4__ = __webpack_require__(/*! ./constants */ "./node_modules/ngx-bootstrap/chronos/units/constants.js");
/* harmony import */ var _utils_type_checks__WEBPACK_IMPORTED_MODULE_5__ = __webpack_require__(/*! ../utils/type-checks */ "./node_modules/ngx-bootstrap/chronos/utils/type-checks.js");
/* harmony import */ var _priorities__WEBPACK_IMPORTED_MODULE_6__ = __webpack_require__(/*! ./priorities */ "./node_modules/ngx-bootstrap/chronos/units/priorities.js");
/* harmony import */ var _aliases__WEBPACK_IMPORTED_MODULE_7__ = __webpack_require__(/*! ./aliases */ "./node_modules/ngx-bootstrap/chronos/units/aliases.js");








// FORMATTING
function getYear(date, opts) {
    return Object(_utils_date_getters__WEBPACK_IMPORTED_MODULE_1__["getFullYear"])(date, opts.isUTC).toString();
}
Object(_format_format__WEBPACK_IMPORTED_MODULE_0__["addFormatToken"])('Y', null, null, function (date, opts) {
    var y = Object(_utils_date_getters__WEBPACK_IMPORTED_MODULE_1__["getFullYear"])(date, opts.isUTC);
    return y <= 9999 ? y.toString(10) : "+" + y;
});
Object(_format_format__WEBPACK_IMPORTED_MODULE_0__["addFormatToken"])(null, ['YY', 2, false], null, function (date, opts) {
    return (Object(_utils_date_getters__WEBPACK_IMPORTED_MODULE_1__["getFullYear"])(date, opts.isUTC) % 100).toString(10);
});
Object(_format_format__WEBPACK_IMPORTED_MODULE_0__["addFormatToken"])(null, ['YYYY', 4, false], null, getYear);
Object(_format_format__WEBPACK_IMPORTED_MODULE_0__["addFormatToken"])(null, ['YYYYY', 5, false], null, getYear);
Object(_format_format__WEBPACK_IMPORTED_MODULE_0__["addFormatToken"])(null, ['YYYYYY', 6, true], null, getYear);
// ALIASES
Object(_aliases__WEBPACK_IMPORTED_MODULE_7__["addUnitAlias"])('year', 'y');
// PRIORITIES
Object(_priorities__WEBPACK_IMPORTED_MODULE_6__["addUnitPriority"])('year', 1);
// PARSING
Object(_parse_regex__WEBPACK_IMPORTED_MODULE_2__["addRegexToken"])('Y', _parse_regex__WEBPACK_IMPORTED_MODULE_2__["matchSigned"]);
Object(_parse_regex__WEBPACK_IMPORTED_MODULE_2__["addRegexToken"])('YY', _parse_regex__WEBPACK_IMPORTED_MODULE_2__["match1to2"], _parse_regex__WEBPACK_IMPORTED_MODULE_2__["match2"]);
Object(_parse_regex__WEBPACK_IMPORTED_MODULE_2__["addRegexToken"])('YYYY', _parse_regex__WEBPACK_IMPORTED_MODULE_2__["match1to4"], _parse_regex__WEBPACK_IMPORTED_MODULE_2__["match4"]);
Object(_parse_regex__WEBPACK_IMPORTED_MODULE_2__["addRegexToken"])('YYYYY', _parse_regex__WEBPACK_IMPORTED_MODULE_2__["match1to6"], _parse_regex__WEBPACK_IMPORTED_MODULE_2__["match6"]);
Object(_parse_regex__WEBPACK_IMPORTED_MODULE_2__["addRegexToken"])('YYYYYY', _parse_regex__WEBPACK_IMPORTED_MODULE_2__["match1to6"], _parse_regex__WEBPACK_IMPORTED_MODULE_2__["match6"]);
Object(_parse_token__WEBPACK_IMPORTED_MODULE_3__["addParseToken"])(['YYYYY', 'YYYYYY'], _constants__WEBPACK_IMPORTED_MODULE_4__["YEAR"]);
Object(_parse_token__WEBPACK_IMPORTED_MODULE_3__["addParseToken"])('YYYY', function (input, array, config) {
    array[_constants__WEBPACK_IMPORTED_MODULE_4__["YEAR"]] = input.length === 2 ? parseTwoDigitYear(input) : Object(_utils_type_checks__WEBPACK_IMPORTED_MODULE_5__["toInt"])(input);
    return config;
});
Object(_parse_token__WEBPACK_IMPORTED_MODULE_3__["addParseToken"])('YY', function (input, array, config) {
    array[_constants__WEBPACK_IMPORTED_MODULE_4__["YEAR"]] = parseTwoDigitYear(input);
    return config;
});
Object(_parse_token__WEBPACK_IMPORTED_MODULE_3__["addParseToken"])('Y', function (input, array, config) {
    array[_constants__WEBPACK_IMPORTED_MODULE_4__["YEAR"]] = parseInt(input, 10);
    return config;
});
function parseTwoDigitYear(input) {
    return Object(_utils_type_checks__WEBPACK_IMPORTED_MODULE_5__["toInt"])(input) + (Object(_utils_type_checks__WEBPACK_IMPORTED_MODULE_5__["toInt"])(input) > 68 ? 1900 : 2000);
}
function daysInYear(year) {
    return isLeapYear(year) ? 366 : 365;
}
function isLeapYear(year) {
    return (year % 4 === 0 && year % 100 !== 0) || year % 400 === 0;
}
//# sourceMappingURL=year.js.map

/***/ }),

/***/ "./node_modules/ngx-bootstrap/chronos/utils.js":
/*!*****************************************************!*\
  !*** ./node_modules/ngx-bootstrap/chronos/utils.js ***!
  \*****************************************************/
/*! exports provided: mod, absFloor */
/***/ (function(module, __webpack_exports__, __webpack_require__) {

"use strict";
__webpack_require__.r(__webpack_exports__);
/* harmony export (binding) */ __webpack_require__.d(__webpack_exports__, "mod", function() { return mod; });
/* harmony export (binding) */ __webpack_require__.d(__webpack_exports__, "absFloor", function() { return absFloor; });
function mod(n, x) {
    return (n % x + x) % x;
}
function absFloor(num) {
    return num < 0 ? Math.ceil(num) || 0 : Math.floor(num);
}
//# sourceMappingURL=utils.js.map

/***/ }),

/***/ "./node_modules/ngx-bootstrap/chronos/utils/abs-ceil.js":
/*!**************************************************************!*\
  !*** ./node_modules/ngx-bootstrap/chronos/utils/abs-ceil.js ***!
  \**************************************************************/
/*! exports provided: absCeil */
/***/ (function(module, __webpack_exports__, __webpack_require__) {

"use strict";
__webpack_require__.r(__webpack_exports__);
/* harmony export (binding) */ __webpack_require__.d(__webpack_exports__, "absCeil", function() { return absCeil; });
function absCeil(number) {
    return number < 0 ? Math.floor(number) : Math.ceil(number);
}
//# sourceMappingURL=abs-ceil.js.map

/***/ }),

/***/ "./node_modules/ngx-bootstrap/chronos/utils/abs-round.js":
/*!***************************************************************!*\
  !*** ./node_modules/ngx-bootstrap/chronos/utils/abs-round.js ***!
  \***************************************************************/
/*! exports provided: absRound */
/***/ (function(module, __webpack_exports__, __webpack_require__) {

"use strict";
__webpack_require__.r(__webpack_exports__);
/* harmony export (binding) */ __webpack_require__.d(__webpack_exports__, "absRound", function() { return absRound; });
function absRound(num) {
    return num < 0 ? Math.round(num * -1) * -1 : Math.round(num);
}
//# sourceMappingURL=abs-round.js.map

/***/ }),

/***/ "./node_modules/ngx-bootstrap/chronos/utils/compare-arrays.js":
/*!********************************************************************!*\
  !*** ./node_modules/ngx-bootstrap/chronos/utils/compare-arrays.js ***!
  \********************************************************************/
/*! exports provided: compareArrays */
/***/ (function(module, __webpack_exports__, __webpack_require__) {

"use strict";
__webpack_require__.r(__webpack_exports__);
/* harmony export (binding) */ __webpack_require__.d(__webpack_exports__, "compareArrays", function() { return compareArrays; });
/* harmony import */ var _type_checks__WEBPACK_IMPORTED_MODULE_0__ = __webpack_require__(/*! ./type-checks */ "./node_modules/ngx-bootstrap/chronos/utils/type-checks.js");

function compareArrays(array1, array2, dontConvert) {
    var len = Math.min(array1.length, array2.length);
    var lengthDiff = Math.abs(array1.length - array2.length);
    var diffs = 0;
    var i;
    for (i = 0; i < len; i++) {
        if ((dontConvert && array1[i] !== array2[i])
            || (!dontConvert && Object(_type_checks__WEBPACK_IMPORTED_MODULE_0__["toInt"])(array1[i]) !== Object(_type_checks__WEBPACK_IMPORTED_MODULE_0__["toInt"])(array2[i]))) {
            diffs++;
        }
    }
    return diffs + lengthDiff;
}
//# sourceMappingURL=compare-arrays.js.map

/***/ }),

/***/ "./node_modules/ngx-bootstrap/chronos/utils/date-compare.js":
/*!******************************************************************!*\
  !*** ./node_modules/ngx-bootstrap/chronos/utils/date-compare.js ***!
  \******************************************************************/
/*! exports provided: isAfter, isBefore, isBetween, isSame, isSameOrAfter, isSameOrBefore */
/***/ (function(module, __webpack_exports__, __webpack_require__) {

"use strict";
__webpack_require__.r(__webpack_exports__);
/* harmony export (binding) */ __webpack_require__.d(__webpack_exports__, "isAfter", function() { return isAfter; });
/* harmony export (binding) */ __webpack_require__.d(__webpack_exports__, "isBefore", function() { return isBefore; });
/* harmony export (binding) */ __webpack_require__.d(__webpack_exports__, "isBetween", function() { return isBetween; });
/* harmony export (binding) */ __webpack_require__.d(__webpack_exports__, "isSame", function() { return isSame; });
/* harmony export (binding) */ __webpack_require__.d(__webpack_exports__, "isSameOrAfter", function() { return isSameOrAfter; });
/* harmony export (binding) */ __webpack_require__.d(__webpack_exports__, "isSameOrBefore", function() { return isSameOrBefore; });
/* harmony import */ var _start_end_of__WEBPACK_IMPORTED_MODULE_0__ = __webpack_require__(/*! ./start-end-of */ "./node_modules/ngx-bootstrap/chronos/utils/start-end-of.js");

function isAfter(date1, date2, units) {
    if (units === void 0) { units = 'milliseconds'; }
    if (!date1 || !date2) {
        return false;
    }
    if (units === 'milliseconds') {
        return date1.valueOf() > date2.valueOf();
    }
    return date2.valueOf() < Object(_start_end_of__WEBPACK_IMPORTED_MODULE_0__["startOf"])(date1, units).valueOf();
}
function isBefore(date1, date2, units) {
    if (units === void 0) { units = 'milliseconds'; }
    if (!date1 || !date2) {
        return false;
    }
    if (units === 'milliseconds') {
        return date1.valueOf() < date2.valueOf();
    }
    return Object(_start_end_of__WEBPACK_IMPORTED_MODULE_0__["endOf"])(date1, units).valueOf() < date2.valueOf();
}
function isBetween(date, from, to, units, inclusivity) {
    if (inclusivity === void 0) { inclusivity = '()'; }
    var leftBound = inclusivity[0] === '('
        ? isAfter(date, from, units)
        : !isBefore(date, from, units);
    var rightBound = inclusivity[1] === ')'
        ? isBefore(date, to, units)
        : !isAfter(date, to, units);
    return leftBound && rightBound;
}
function isSame(date1, date2, units) {
    if (units === void 0) { units = 'milliseconds'; }
    if (!date1 || !date2) {
        return false;
    }
    if (units === 'milliseconds') {
        return date1.valueOf() === date2.valueOf();
    }
    var inputMs = date2.valueOf();
    return (Object(_start_end_of__WEBPACK_IMPORTED_MODULE_0__["startOf"])(date1, units).valueOf() <= inputMs &&
        inputMs <= Object(_start_end_of__WEBPACK_IMPORTED_MODULE_0__["endOf"])(date1, units).valueOf());
}
function isSameOrAfter(date1, date2, units) {
    return isSame(date1, date2, units) || isAfter(date1, date2, units);
}
function isSameOrBefore(date1, date2, units) {
    return isSame(date1, date2, units) || isBefore(date1, date2, units);
}
//# sourceMappingURL=date-compare.js.map

/***/ }),

/***/ "./node_modules/ngx-bootstrap/chronos/utils/date-getters.js":
/*!******************************************************************!*\
  !*** ./node_modules/ngx-bootstrap/chronos/utils/date-getters.js ***!
  \******************************************************************/
/*! exports provided: getHours, getMinutes, getSeconds, getMilliseconds, getTime, getDay, getDate, getMonth, getFullYear, getUnixTime, unix, getFirstDayOfMonth, daysInMonth, _daysInMonth, isFirstDayOfWeek, isSameMonth, isSameYear, isSameDay */
/***/ (function(module, __webpack_exports__, __webpack_require__) {

"use strict";
__webpack_require__.r(__webpack_exports__);
/* harmony export (binding) */ __webpack_require__.d(__webpack_exports__, "getHours", function() { return getHours; });
/* harmony export (binding) */ __webpack_require__.d(__webpack_exports__, "getMinutes", function() { return getMinutes; });
/* harmony export (binding) */ __webpack_require__.d(__webpack_exports__, "getSeconds", function() { return getSeconds; });
/* harmony export (binding) */ __webpack_require__.d(__webpack_exports__, "getMilliseconds", function() { return getMilliseconds; });
/* harmony export (binding) */ __webpack_require__.d(__webpack_exports__, "getTime", function() { return getTime; });
/* harmony export (binding) */ __webpack_require__.d(__webpack_exports__, "getDay", function() { return getDay; });
/* harmony export (binding) */ __webpack_require__.d(__webpack_exports__, "getDate", function() { return getDate; });
/* harmony export (binding) */ __webpack_require__.d(__webpack_exports__, "getMonth", function() { return getMonth; });
/* harmony export (binding) */ __webpack_require__.d(__webpack_exports__, "getFullYear", function() { return getFullYear; });
/* harmony export (binding) */ __webpack_require__.d(__webpack_exports__, "getUnixTime", function() { return getUnixTime; });
/* harmony export (binding) */ __webpack_require__.d(__webpack_exports__, "unix", function() { return unix; });
/* harmony export (binding) */ __webpack_require__.d(__webpack_exports__, "getFirstDayOfMonth", function() { return getFirstDayOfMonth; });
/* harmony export (binding) */ __webpack_require__.d(__webpack_exports__, "daysInMonth", function() { return daysInMonth; });
/* harmony export (binding) */ __webpack_require__.d(__webpack_exports__, "_daysInMonth", function() { return _daysInMonth; });
/* harmony export (binding) */ __webpack_require__.d(__webpack_exports__, "isFirstDayOfWeek", function() { return isFirstDayOfWeek; });
/* harmony export (binding) */ __webpack_require__.d(__webpack_exports__, "isSameMonth", function() { return isSameMonth; });
/* harmony export (binding) */ __webpack_require__.d(__webpack_exports__, "isSameYear", function() { return isSameYear; });
/* harmony export (binding) */ __webpack_require__.d(__webpack_exports__, "isSameDay", function() { return isSameDay; });
/* harmony import */ var _create_date_from_array__WEBPACK_IMPORTED_MODULE_0__ = __webpack_require__(/*! ../create/date-from-array */ "./node_modules/ngx-bootstrap/chronos/create/date-from-array.js");

function getHours(date, isUTC) {
    if (isUTC === void 0) { isUTC = false; }
    return isUTC ? date.getUTCHours() : date.getHours();
}
function getMinutes(date, isUTC) {
    if (isUTC === void 0) { isUTC = false; }
    return isUTC ? date.getUTCMinutes() : date.getMinutes();
}
function getSeconds(date, isUTC) {
    if (isUTC === void 0) { isUTC = false; }
    return isUTC ? date.getUTCSeconds() : date.getSeconds();
}
function getMilliseconds(date, isUTC) {
    if (isUTC === void 0) { isUTC = false; }
    return isUTC ? date.getUTCMilliseconds() : date.getMilliseconds();
}
function getTime(date) {
    return date.getTime();
}
function getDay(date, isUTC) {
    if (isUTC === void 0) { isUTC = false; }
    return isUTC ? date.getUTCDay() : date.getDay();
}
function getDate(date, isUTC) {
    if (isUTC === void 0) { isUTC = false; }
    return isUTC ? date.getUTCDate() : date.getDate();
}
function getMonth(date, isUTC) {
    if (isUTC === void 0) { isUTC = false; }
    return isUTC ? date.getUTCMonth() : date.getMonth();
}
function getFullYear(date, isUTC) {
    if (isUTC === void 0) { isUTC = false; }
    return isUTC ? date.getUTCFullYear() : date.getFullYear();
}
function getUnixTime(date) {
    return Math.floor(date.valueOf() / 1000);
}
function unix(date) {
    return Math.floor(date.valueOf() / 1000);
}
function getFirstDayOfMonth(date) {
    return Object(_create_date_from_array__WEBPACK_IMPORTED_MODULE_0__["createDate"])(date.getFullYear(), date.getMonth(), 1, date.getHours(), date.getMinutes(), date.getSeconds());
}
function daysInMonth(date) {
    return _daysInMonth(date.getFullYear(), date.getMonth());
}
function _daysInMonth(year, month) {
    return new Date(Date.UTC(year, month + 1, 0)).getUTCDate();
}
function isFirstDayOfWeek(date, firstDayOfWeek) {
    return date.getDay() === firstDayOfWeek;
}
function isSameMonth(date1, date2) {
    if (!date1 || !date2) {
        return false;
    }
    return isSameYear(date1, date2) && getMonth(date1) === getMonth(date2);
}
function isSameYear(date1, date2) {
    if (!date1 || !date2) {
        return false;
    }
    return getFullYear(date1) === getFullYear(date2);
}
function isSameDay(date1, date2) {
    if (!date1 || !date2) {
        return false;
    }
    return (isSameYear(date1, date2) &&
        isSameMonth(date1, date2) &&
        getDate(date1) === getDate(date2));
}
//# sourceMappingURL=date-getters.js.map

/***/ }),

/***/ "./node_modules/ngx-bootstrap/chronos/utils/date-setters.js":
/*!******************************************************************!*\
  !*** ./node_modules/ngx-bootstrap/chronos/utils/date-setters.js ***!
  \******************************************************************/
/*! exports provided: shiftDate, setFullDate, setFullYear, setMonth, setDay, setHours, setMinutes, setSeconds, setMilliseconds, setDate, setTime */
/***/ (function(module, __webpack_exports__, __webpack_require__) {

"use strict";
__webpack_require__.r(__webpack_exports__);
/* harmony export (binding) */ __webpack_require__.d(__webpack_exports__, "shiftDate", function() { return shiftDate; });
/* harmony export (binding) */ __webpack_require__.d(__webpack_exports__, "setFullDate", function() { return setFullDate; });
/* harmony export (binding) */ __webpack_require__.d(__webpack_exports__, "setFullYear", function() { return setFullYear; });
/* harmony export (binding) */ __webpack_require__.d(__webpack_exports__, "setMonth", function() { return setMonth; });
/* harmony export (binding) */ __webpack_require__.d(__webpack_exports__, "setDay", function() { return setDay; });
/* harmony export (binding) */ __webpack_require__.d(__webpack_exports__, "setHours", function() { return setHours; });
/* harmony export (binding) */ __webpack_require__.d(__webpack_exports__, "setMinutes", function() { return setMinutes; });
/* harmony export (binding) */ __webpack_require__.d(__webpack_exports__, "setSeconds", function() { return setSeconds; });
/* harmony export (binding) */ __webpack_require__.d(__webpack_exports__, "setMilliseconds", function() { return setMilliseconds; });
/* harmony export (binding) */ __webpack_require__.d(__webpack_exports__, "setDate", function() { return setDate; });
/* harmony export (binding) */ __webpack_require__.d(__webpack_exports__, "setTime", function() { return setTime; });
/* harmony import */ var _units_month__WEBPACK_IMPORTED_MODULE_0__ = __webpack_require__(/*! ../units/month */ "./node_modules/ngx-bootstrap/chronos/units/month.js");
/* harmony import */ var _type_checks__WEBPACK_IMPORTED_MODULE_1__ = __webpack_require__(/*! ./type-checks */ "./node_modules/ngx-bootstrap/chronos/utils/type-checks.js");
/* harmony import */ var _date_getters__WEBPACK_IMPORTED_MODULE_2__ = __webpack_require__(/*! ./date-getters */ "./node_modules/ngx-bootstrap/chronos/utils/date-getters.js");
/* harmony import */ var _units_year__WEBPACK_IMPORTED_MODULE_3__ = __webpack_require__(/*! ../units/year */ "./node_modules/ngx-bootstrap/chronos/units/year.js");
/* harmony import */ var _create_date_from_array__WEBPACK_IMPORTED_MODULE_4__ = __webpack_require__(/*! ../create/date-from-array */ "./node_modules/ngx-bootstrap/chronos/create/date-from-array.js");





var defaultTimeUnit = {
    year: 0,
    month: 0,
    day: 0,
    hour: 0,
    minute: 0,
    seconds: 0
};
function shiftDate(date, unit) {
    var _unit = Object.assign({}, defaultTimeUnit, unit);
    var year = date.getFullYear() + (_unit.year || 0);
    var month = date.getMonth() + (_unit.month || 0);
    var day = date.getDate() + (_unit.day || 0);
    if (_unit.month && !_unit.day) {
        day = Math.min(day, Object(_units_month__WEBPACK_IMPORTED_MODULE_0__["daysInMonth"])(year, month));
    }
    return Object(_create_date_from_array__WEBPACK_IMPORTED_MODULE_4__["createDate"])(year, month, day, date.getHours() + (_unit.hour || 0), date.getMinutes() + (_unit.minute || 0), date.getSeconds() + (_unit.seconds || 0));
}
function setFullDate(date, unit) {
    return Object(_create_date_from_array__WEBPACK_IMPORTED_MODULE_4__["createDate"])(getNum(date.getFullYear(), unit.year), getNum(date.getMonth(), unit.month), getNum(date.getDate(), unit.day), getNum(date.getHours(), unit.hour), getNum(date.getMinutes(), unit.minute), getNum(date.getSeconds(), unit.seconds), getNum(date.getMilliseconds(), unit.milliseconds));
}
function getNum(def, num) {
    return Object(_type_checks__WEBPACK_IMPORTED_MODULE_1__["isNumber"])(num) ? num : def;
}
function setFullYear(date, value, isUTC) {
    var _month = Object(_date_getters__WEBPACK_IMPORTED_MODULE_2__["getMonth"])(date, isUTC);
    var _date = Object(_date_getters__WEBPACK_IMPORTED_MODULE_2__["getDate"])(date, isUTC);
    var _year = Object(_date_getters__WEBPACK_IMPORTED_MODULE_2__["getFullYear"])(date, isUTC);
    if (Object(_units_year__WEBPACK_IMPORTED_MODULE_3__["isLeapYear"])(_year) && _month === 1 && _date === 29) {
        var _daysInMonth = Object(_units_month__WEBPACK_IMPORTED_MODULE_0__["daysInMonth"])(value, _month);
        isUTC ? date.setUTCFullYear(value, _month, _daysInMonth) : date.setFullYear(value, _month, _daysInMonth);
    }
    isUTC ? date.setUTCFullYear(value) : date.setFullYear(value);
    return date;
}
function setMonth(date, value, isUTC) {
    var dayOfMonth = Math.min(Object(_date_getters__WEBPACK_IMPORTED_MODULE_2__["getDate"])(date), Object(_units_month__WEBPACK_IMPORTED_MODULE_0__["daysInMonth"])(Object(_date_getters__WEBPACK_IMPORTED_MODULE_2__["getFullYear"])(date), value));
    isUTC ? date.setUTCMonth(value, dayOfMonth) : date.setMonth(value, dayOfMonth);
    return date;
}
function setDay(date, value, isUTC) {
    isUTC ? date.setUTCDate(value) : date.setDate(value);
    return date;
}
function setHours(date, value, isUTC) {
    isUTC ? date.setUTCHours(value) : date.setHours(value);
    return date;
}
function setMinutes(date, value, isUTC) {
    isUTC ? date.setUTCMinutes(value) : date.setMinutes(value);
    return date;
}
function setSeconds(date, value, isUTC) {
    isUTC ? date.setUTCSeconds(value) : date.setSeconds(value);
    return date;
}
function setMilliseconds(date, value, isUTC) {
    isUTC ? date.setUTCMilliseconds(value) : date.setMilliseconds(value);
    return date;
}
function setDate(date, value, isUTC) {
    isUTC ? date.setUTCDate(value) : date.setDate(value);
    return date;
}
function setTime(date, value) {
    date.setTime(value);
    return date;
}
//# sourceMappingURL=date-setters.js.map

/***/ }),

/***/ "./node_modules/ngx-bootstrap/chronos/utils/defaults.js":
/*!**************************************************************!*\
  !*** ./node_modules/ngx-bootstrap/chronos/utils/defaults.js ***!
  \**************************************************************/
/*! exports provided: defaults */
/***/ (function(module, __webpack_exports__, __webpack_require__) {

"use strict";
__webpack_require__.r(__webpack_exports__);
/* harmony export (binding) */ __webpack_require__.d(__webpack_exports__, "defaults", function() { return defaults; });
// Pick the first defined of two or three arguments.
function defaults(a, b, c) {
    if (a != null) {
        return a;
    }
    if (b != null) {
        return b;
    }
    return c;
}
//# sourceMappingURL=defaults.js.map

/***/ }),

/***/ "./node_modules/ngx-bootstrap/chronos/utils/start-end-of.js":
/*!******************************************************************!*\
  !*** ./node_modules/ngx-bootstrap/chronos/utils/start-end-of.js ***!
  \******************************************************************/
/*! exports provided: startOf, endOf */
/***/ (function(module, __webpack_exports__, __webpack_require__) {

"use strict";
__webpack_require__.r(__webpack_exports__);
/* harmony export (binding) */ __webpack_require__.d(__webpack_exports__, "startOf", function() { return startOf; });
/* harmony export (binding) */ __webpack_require__.d(__webpack_exports__, "endOf", function() { return endOf; });
/* harmony import */ var _date_setters__WEBPACK_IMPORTED_MODULE_0__ = __webpack_require__(/*! ./date-setters */ "./node_modules/ngx-bootstrap/chronos/utils/date-setters.js");
/* harmony import */ var _create_clone__WEBPACK_IMPORTED_MODULE_1__ = __webpack_require__(/*! ../create/clone */ "./node_modules/ngx-bootstrap/chronos/create/clone.js");
/* harmony import */ var _units_day_of_week__WEBPACK_IMPORTED_MODULE_2__ = __webpack_require__(/*! ../units/day-of-week */ "./node_modules/ngx-bootstrap/chronos/units/day-of-week.js");
/* harmony import */ var _date_getters__WEBPACK_IMPORTED_MODULE_3__ = __webpack_require__(/*! ./date-getters */ "./node_modules/ngx-bootstrap/chronos/utils/date-getters.js");
/* harmony import */ var _moment_add_subtract__WEBPACK_IMPORTED_MODULE_4__ = __webpack_require__(/*! ../moment/add-subtract */ "./node_modules/ngx-bootstrap/chronos/moment/add-subtract.js");





function startOf(date, unit, isUTC) {
    var _date = Object(_create_clone__WEBPACK_IMPORTED_MODULE_1__["cloneDate"])(date);
    // the following switch intentionally omits break keywords
    // to utilize falling through the cases.
    switch (unit) {
        case 'year':
            Object(_date_setters__WEBPACK_IMPORTED_MODULE_0__["setMonth"])(_date, 0, isUTC);
        /* falls through */
        case 'quarter':
        case 'month':
            Object(_date_setters__WEBPACK_IMPORTED_MODULE_0__["setDate"])(_date, 1, isUTC);
        /* falls through */
        case 'week':
        case 'isoWeek':
        case 'day':
        case 'date':
            Object(_date_setters__WEBPACK_IMPORTED_MODULE_0__["setHours"])(_date, 0, isUTC);
        /* falls through */
        case 'hours':
            Object(_date_setters__WEBPACK_IMPORTED_MODULE_0__["setMinutes"])(_date, 0, isUTC);
        /* falls through */
        case 'minutes':
            Object(_date_setters__WEBPACK_IMPORTED_MODULE_0__["setSeconds"])(_date, 0, isUTC);
        /* falls through */
        case 'seconds':
            Object(_date_setters__WEBPACK_IMPORTED_MODULE_0__["setMilliseconds"])(_date, 0, isUTC);
    }
    // weeks are a special case
    if (unit === 'week') {
        Object(_units_day_of_week__WEBPACK_IMPORTED_MODULE_2__["setLocaleDayOfWeek"])(_date, 0, { isUTC: isUTC });
    }
    if (unit === 'isoWeek') {
        Object(_units_day_of_week__WEBPACK_IMPORTED_MODULE_2__["setISODayOfWeek"])(_date, 1);
    }
    // quarters are also special
    if (unit === 'quarter') {
        Object(_date_setters__WEBPACK_IMPORTED_MODULE_0__["setMonth"])(_date, Math.floor(Object(_date_getters__WEBPACK_IMPORTED_MODULE_3__["getMonth"])(_date, isUTC) / 3) * 3, isUTC);
    }
    return _date;
}
function endOf(date, unit, isUTC) {
    var _unit = unit;
    // 'date' is an alias for 'day', so it should be considered as such.
    if (_unit === 'date') {
        _unit = 'day';
    }
    var start = startOf(date, _unit, isUTC);
    var _step = Object(_moment_add_subtract__WEBPACK_IMPORTED_MODULE_4__["add"])(start, 1, _unit === 'isoWeek' ? 'week' : _unit, isUTC);
    var res = Object(_moment_add_subtract__WEBPACK_IMPORTED_MODULE_4__["subtract"])(_step, 1, 'milliseconds', isUTC);
    return res;
}
//# sourceMappingURL=start-end-of.js.map

/***/ }),

/***/ "./node_modules/ngx-bootstrap/chronos/utils/type-checks.js":
/*!*****************************************************************!*\
  !*** ./node_modules/ngx-bootstrap/chronos/utils/type-checks.js ***!
  \*****************************************************************/
/*! exports provided: isString, isDate, isBoolean, isDateValid, isFunction, isNumber, isArray, hasOwnProp, isObject, isObjectEmpty, isUndefined, toInt */
/***/ (function(module, __webpack_exports__, __webpack_require__) {

"use strict";
__webpack_require__.r(__webpack_exports__);
/* harmony export (binding) */ __webpack_require__.d(__webpack_exports__, "isString", function() { return isString; });
/* harmony export (binding) */ __webpack_require__.d(__webpack_exports__, "isDate", function() { return isDate; });
/* harmony export (binding) */ __webpack_require__.d(__webpack_exports__, "isBoolean", function() { return isBoolean; });
/* harmony export (binding) */ __webpack_require__.d(__webpack_exports__, "isDateValid", function() { return isDateValid; });
/* harmony export (binding) */ __webpack_require__.d(__webpack_exports__, "isFunction", function() { return isFunction; });
/* harmony export (binding) */ __webpack_require__.d(__webpack_exports__, "isNumber", function() { return isNumber; });
/* harmony export (binding) */ __webpack_require__.d(__webpack_exports__, "isArray", function() { return isArray; });
/* harmony export (binding) */ __webpack_require__.d(__webpack_exports__, "hasOwnProp", function() { return hasOwnProp; });
/* harmony export (binding) */ __webpack_require__.d(__webpack_exports__, "isObject", function() { return isObject; });
/* harmony export (binding) */ __webpack_require__.d(__webpack_exports__, "isObjectEmpty", function() { return isObjectEmpty; });
/* harmony export (binding) */ __webpack_require__.d(__webpack_exports__, "isUndefined", function() { return isUndefined; });
/* harmony export (binding) */ __webpack_require__.d(__webpack_exports__, "toInt", function() { return toInt; });
/* harmony import */ var _utils__WEBPACK_IMPORTED_MODULE_0__ = __webpack_require__(/*! ../utils */ "./node_modules/ngx-bootstrap/chronos/utils.js");

function isString(str) {
    return typeof str === 'string';
}
function isDate(value) {
    return value instanceof Date || Object.prototype.toString.call(value) === '[object Date]';
}
function isBoolean(value) {
    return value === true || value === false;
}
function isDateValid(date) {
    return date && date.getTime && !isNaN(date.getTime());
}
function isFunction(fn) {
    return (fn instanceof Function ||
        Object.prototype.toString.call(fn) === '[object Function]');
}
function isNumber(value) {
    return typeof value === 'number' || Object.prototype.toString.call(value) === '[object Number]';
}
function isArray(input) {
    return (input instanceof Array ||
        Object.prototype.toString.call(input) === '[object Array]');
}
function hasOwnProp(a /*object*/, b) {
    return Object.prototype.hasOwnProperty.call(a, b);
}
function isObject(input /*object*/) {
    // IE8 will treat undefined and null as object if it wasn't for
    // input != null
    return (input != null && Object.prototype.toString.call(input) === '[object Object]');
}
function isObjectEmpty(obj) {
    if (Object.getOwnPropertyNames) {
        return (Object.getOwnPropertyNames(obj).length === 0);
    }
    var k;
    for (k in obj) {
        if (obj.hasOwnProperty(k)) {
            return false;
        }
    }
    return true;
}
function isUndefined(input) {
    return input === void 0;
}
function toInt(argumentForCoercion) {
    var coercedNumber = +argumentForCoercion;
    var value = 0;
    if (coercedNumber !== 0 && isFinite(coercedNumber)) {
        value = Object(_utils__WEBPACK_IMPORTED_MODULE_0__["absFloor"])(coercedNumber);
    }
    return value;
}
//# sourceMappingURL=type-checks.js.map

/***/ }),

/***/ "./node_modules/ngx-bootstrap/chronos/utils/zero-fill.js":
/*!***************************************************************!*\
  !*** ./node_modules/ngx-bootstrap/chronos/utils/zero-fill.js ***!
  \***************************************************************/
/*! exports provided: zeroFill */
/***/ (function(module, __webpack_exports__, __webpack_require__) {

"use strict";
__webpack_require__.r(__webpack_exports__);
/* harmony export (binding) */ __webpack_require__.d(__webpack_exports__, "zeroFill", function() { return zeroFill; });
function zeroFill(num, targetLength, forceSign) {
    var absNumber = "" + Math.abs(num);
    var zerosToFill = targetLength - absNumber.length;
    var sign = num >= 0;
    var _sign = sign ? (forceSign ? '+' : '') : '-';
    // todo: this is crazy slow
    var _zeros = Math.pow(10, Math.max(0, zerosToFill)).toString().substr(1);
    return (_sign + _zeros + absNumber);
}
//# sourceMappingURL=zero-fill.js.map

/***/ }),

/***/ "./node_modules/ngx-bootstrap/datepicker/base/bs-datepicker-container.js":
/*!*******************************************************************************!*\
  !*** ./node_modules/ngx-bootstrap/datepicker/base/bs-datepicker-container.js ***!
  \*******************************************************************************/
/*! exports provided: BsDatepickerAbstractComponent */
/***/ (function(module, __webpack_exports__, __webpack_require__) {

"use strict";
__webpack_require__.r(__webpack_exports__);
/* harmony export (binding) */ __webpack_require__.d(__webpack_exports__, "BsDatepickerAbstractComponent", function() { return BsDatepickerAbstractComponent; });
var BsDatepickerAbstractComponent = /** @class */ (function () {
    function BsDatepickerAbstractComponent() {
        this._customRangesFish = [];
    }
    Object.defineProperty(BsDatepickerAbstractComponent.prototype, "minDate", {
        set: function (value) {
            this._effects.setMinDate(value);
        },
        enumerable: true,
        configurable: true
    });
    Object.defineProperty(BsDatepickerAbstractComponent.prototype, "maxDate", {
        set: function (value) {
            this._effects.setMaxDate(value);
        },
        enumerable: true,
        configurable: true
    });
    Object.defineProperty(BsDatepickerAbstractComponent.prototype, "isDisabled", {
        set: function (value) {
            this._effects.setDisabled(value);
        },
        enumerable: true,
        configurable: true
    });
    BsDatepickerAbstractComponent.prototype.setViewMode = function (event) { };
    BsDatepickerAbstractComponent.prototype.navigateTo = function (event) { };
    BsDatepickerAbstractComponent.prototype.dayHoverHandler = function (event) { };
    BsDatepickerAbstractComponent.prototype.monthHoverHandler = function (event) { };
    BsDatepickerAbstractComponent.prototype.yearHoverHandler = function (event) { };
    BsDatepickerAbstractComponent.prototype.daySelectHandler = function (day) { };
    BsDatepickerAbstractComponent.prototype.monthSelectHandler = function (event) { };
    BsDatepickerAbstractComponent.prototype.yearSelectHandler = function (event) { };
    BsDatepickerAbstractComponent.prototype._stopPropagation = function (event) {
        event.stopPropagation();
    };
    return BsDatepickerAbstractComponent;
}());

//# sourceMappingURL=bs-datepicker-container.js.map

/***/ }),

/***/ "./node_modules/ngx-bootstrap/datepicker/bs-datepicker-input.directive.js":
/*!********************************************************************************!*\
  !*** ./node_modules/ngx-bootstrap/datepicker/bs-datepicker-input.directive.js ***!
  \********************************************************************************/
/*! exports provided: BsDatepickerInputDirective */
/***/ (function(module, __webpack_exports__, __webpack_require__) {

"use strict";
__webpack_require__.r(__webpack_exports__);
/* harmony export (binding) */ __webpack_require__.d(__webpack_exports__, "BsDatepickerInputDirective", function() { return BsDatepickerInputDirective; });
/* harmony import */ var _angular_core__WEBPACK_IMPORTED_MODULE_0__ = __webpack_require__(/*! @angular/core */ "./node_modules/@angular/core/fesm5/core.js");
/* harmony import */ var _angular_forms__WEBPACK_IMPORTED_MODULE_1__ = __webpack_require__(/*! @angular/forms */ "./node_modules/@angular/forms/fesm5/forms.js");
/* harmony import */ var _chronos_create_local__WEBPACK_IMPORTED_MODULE_2__ = __webpack_require__(/*! ../chronos/create/local */ "./node_modules/ngx-bootstrap/chronos/create/local.js");
/* harmony import */ var _chronos_format__WEBPACK_IMPORTED_MODULE_3__ = __webpack_require__(/*! ../chronos/format */ "./node_modules/ngx-bootstrap/chronos/format.js");
/* harmony import */ var _chronos_locale_locales__WEBPACK_IMPORTED_MODULE_4__ = __webpack_require__(/*! ../chronos/locale/locales */ "./node_modules/ngx-bootstrap/chronos/locale/locales.js");
/* harmony import */ var _chronos_utils_date_compare__WEBPACK_IMPORTED_MODULE_5__ = __webpack_require__(/*! ../chronos/utils/date-compare */ "./node_modules/ngx-bootstrap/chronos/utils/date-compare.js");
/* harmony import */ var _chronos_utils_type_checks__WEBPACK_IMPORTED_MODULE_6__ = __webpack_require__(/*! ../chronos/utils/type-checks */ "./node_modules/ngx-bootstrap/chronos/utils/type-checks.js");
/* harmony import */ var _bs_datepicker_component__WEBPACK_IMPORTED_MODULE_7__ = __webpack_require__(/*! ./bs-datepicker.component */ "./node_modules/ngx-bootstrap/datepicker/bs-datepicker.component.js");
/* harmony import */ var _bs_locale_service__WEBPACK_IMPORTED_MODULE_8__ = __webpack_require__(/*! ./bs-locale.service */ "./node_modules/ngx-bootstrap/datepicker/bs-locale.service.js");









var BS_DATEPICKER_VALUE_ACCESSOR = {
    provide: _angular_forms__WEBPACK_IMPORTED_MODULE_1__["NG_VALUE_ACCESSOR"],
    // tslint:disable-next-line
    useExisting: Object(_angular_core__WEBPACK_IMPORTED_MODULE_0__["forwardRef"])(function () { return BsDatepickerInputDirective; }),
    multi: true
};
var BS_DATEPICKER_VALIDATOR = {
    provide: _angular_forms__WEBPACK_IMPORTED_MODULE_1__["NG_VALIDATORS"],
    useExisting: Object(_angular_core__WEBPACK_IMPORTED_MODULE_0__["forwardRef"])(function () { return BsDatepickerInputDirective; }),
    multi: true
};
var BsDatepickerInputDirective = /** @class */ (function () {
    function BsDatepickerInputDirective(_picker, _localeService, _renderer, _elRef, changeDetection) {
        var _this = this;
        this._picker = _picker;
        this._localeService = _localeService;
        this._renderer = _renderer;
        this._elRef = _elRef;
        this.changeDetection = changeDetection;
        this._onChange = Function.prototype;
        this._onTouched = Function.prototype;
        this._validatorChange = Function.prototype;
        // update input value on datepicker value update
        this._picker.bsValueChange.subscribe(function (value) {
            _this._setInputValue(value);
            if (_this._value !== value) {
                _this._value = value;
                _this._onChange(value);
                _this._onTouched();
            }
            _this.changeDetection.markForCheck();
        });
        // update input value on locale change
        this._localeService.localeChange.subscribe(function () {
            _this._setInputValue(_this._value);
        });
    }
    BsDatepickerInputDirective.prototype._setInputValue = function (value) {
        var initialDate = !value ? ''
            : Object(_chronos_format__WEBPACK_IMPORTED_MODULE_3__["formatDate"])(value, this._picker._config.dateInputFormat, this._localeService.currentLocale);
        this._renderer.setProperty(this._elRef.nativeElement, 'value', initialDate);
    };
    BsDatepickerInputDirective.prototype.onChange = function (event) {
        this.writeValue(event.target.value);
        this._onChange(this._value);
        this._onTouched();
    };
    BsDatepickerInputDirective.prototype.validate = function (c) {
        var _value = c.value;
        if (_value === null || _value === undefined || _value === '') {
            return null;
        }
        if (Object(_chronos_utils_type_checks__WEBPACK_IMPORTED_MODULE_6__["isDate"])(_value)) {
            var _isDateValid = Object(_chronos_utils_type_checks__WEBPACK_IMPORTED_MODULE_6__["isDateValid"])(_value);
            if (!_isDateValid) {
                return { bsDate: { invalid: _value } };
            }
            if (this._picker && this._picker.minDate && Object(_chronos_utils_date_compare__WEBPACK_IMPORTED_MODULE_5__["isBefore"])(_value, this._picker.minDate, 'date')) {
                return { bsDate: { minDate: this._picker.minDate } };
            }
            if (this._picker && this._picker.maxDate && Object(_chronos_utils_date_compare__WEBPACK_IMPORTED_MODULE_5__["isAfter"])(_value, this._picker.maxDate, 'date')) {
                return { bsDate: { maxDate: this._picker.maxDate } };
            }
        }
    };
    BsDatepickerInputDirective.prototype.registerOnValidatorChange = function (fn) {
        this._validatorChange = fn;
    };
    BsDatepickerInputDirective.prototype.writeValue = function (value) {
        if (!value) {
            this._value = null;
        }
        else {
            var _localeKey = this._localeService.currentLocale;
            var _locale = Object(_chronos_locale_locales__WEBPACK_IMPORTED_MODULE_4__["getLocale"])(_localeKey);
            if (!_locale) {
                throw new Error("Locale \"" + _localeKey + "\" is not defined, please add it with \"defineLocale(...)\"");
            }
            this._value = Object(_chronos_create_local__WEBPACK_IMPORTED_MODULE_2__["parseDate"])(value, this._picker._config.dateInputFormat, this._localeService.currentLocale);
        }
        this._picker.bsValue = this._value;
    };
    BsDatepickerInputDirective.prototype.setDisabledState = function (isDisabled) {
        this._picker.isDisabled = isDisabled;
        if (isDisabled) {
            this._renderer.setAttribute(this._elRef.nativeElement, 'disabled', 'disabled');
            return;
        }
        this._renderer.removeAttribute(this._elRef.nativeElement, 'disabled');
    };
    BsDatepickerInputDirective.prototype.registerOnChange = function (fn) {
        this._onChange = fn;
    };
    BsDatepickerInputDirective.prototype.registerOnTouched = function (fn) {
        this._onTouched = fn;
    };
    BsDatepickerInputDirective.prototype.onBlur = function () {
        this._onTouched();
    };
    BsDatepickerInputDirective.prototype.hide = function () {
        this._picker.hide();
    };
    BsDatepickerInputDirective.decorators = [
        { type: _angular_core__WEBPACK_IMPORTED_MODULE_0__["Directive"], args: [{
                    selector: "input[bsDatepicker]",
                    host: {
                        '(change)': 'onChange($event)',
                        '(keyup.esc)': 'hide()',
                        '(blur)': 'onBlur()'
                    },
                    providers: [BS_DATEPICKER_VALUE_ACCESSOR, BS_DATEPICKER_VALIDATOR]
                },] },
    ];
    /** @nocollapse */
    BsDatepickerInputDirective.ctorParameters = function () { return [
        { type: _bs_datepicker_component__WEBPACK_IMPORTED_MODULE_7__["BsDatepickerDirective"], decorators: [{ type: _angular_core__WEBPACK_IMPORTED_MODULE_0__["Host"] },] },
        { type: _bs_locale_service__WEBPACK_IMPORTED_MODULE_8__["BsLocaleService"], },
        { type: _angular_core__WEBPACK_IMPORTED_MODULE_0__["Renderer2"], },
        { type: _angular_core__WEBPACK_IMPORTED_MODULE_0__["ElementRef"], },
        { type: _angular_core__WEBPACK_IMPORTED_MODULE_0__["ChangeDetectorRef"], },
    ]; };
    return BsDatepickerInputDirective;
}());

//# sourceMappingURL=bs-datepicker-input.directive.js.map

/***/ }),

/***/ "./node_modules/ngx-bootstrap/datepicker/bs-datepicker.component.js":
/*!**************************************************************************!*\
  !*** ./node_modules/ngx-bootstrap/datepicker/bs-datepicker.component.js ***!
  \**************************************************************************/
/*! exports provided: BsDatepickerDirective */
/***/ (function(module, __webpack_exports__, __webpack_require__) {

"use strict";
__webpack_require__.r(__webpack_exports__);
/* harmony export (binding) */ __webpack_require__.d(__webpack_exports__, "BsDatepickerDirective", function() { return BsDatepickerDirective; });
/* harmony import */ var _angular_core__WEBPACK_IMPORTED_MODULE_0__ = __webpack_require__(/*! @angular/core */ "./node_modules/@angular/core/fesm5/core.js");
/* harmony import */ var _component_loader_component_loader_factory__WEBPACK_IMPORTED_MODULE_1__ = __webpack_require__(/*! ../component-loader/component-loader.factory */ "./node_modules/ngx-bootstrap/component-loader/component-loader.factory.js");
/* harmony import */ var _themes_bs_bs_datepicker_container_component__WEBPACK_IMPORTED_MODULE_2__ = __webpack_require__(/*! ./themes/bs/bs-datepicker-container.component */ "./node_modules/ngx-bootstrap/datepicker/themes/bs/bs-datepicker-container.component.js");
/* harmony import */ var _bs_datepicker_config__WEBPACK_IMPORTED_MODULE_3__ = __webpack_require__(/*! ./bs-datepicker.config */ "./node_modules/ngx-bootstrap/datepicker/bs-datepicker.config.js");




var BsDatepickerDirective = /** @class */ (function () {
    function BsDatepickerDirective(_config, _elementRef, _renderer, _viewContainerRef, cis) {
        this._config = _config;
        /**
           * Placement of a datepicker. Accepts: "top", "bottom", "left", "right"
           */
        this.placement = 'bottom';
        /**
           * Specifies events that should trigger. Supports a space separated list of
           * event names.
           */
        this.triggers = 'click';
        /**
           * Close datepicker on outside click
           */
        this.outsideClick = true;
        /**
           * A selector specifying the element the datepicker should be appended to.
           * Currently only supports "body".
           */
        this.container = 'body';
        /**
           * Emits when datepicker value has been changed
           */
        this.bsValueChange = new _angular_core__WEBPACK_IMPORTED_MODULE_0__["EventEmitter"]();
        this._subs = [];
        // todo: assign only subset of fields
        Object.assign(this, this._config);
        this._datepicker = cis.createLoader(_elementRef, _viewContainerRef, _renderer);
        this.onShown = this._datepicker.onShown;
        this.onHidden = this._datepicker.onHidden;
    }
    Object.defineProperty(BsDatepickerDirective.prototype, "isOpen", {
        get: /**
           * Returns whether or not the datepicker is currently being shown
           */
        function () {
            return this._datepicker.isShown;
        },
        set: function (value) {
            if (value) {
                this.show();
            }
            else {
                this.hide();
            }
        },
        enumerable: true,
        configurable: true
    });
    Object.defineProperty(BsDatepickerDirective.prototype, "bsValue", {
        set: /**
           * Initial value of datepicker
           */
        function (value) {
            if (this._bsValue === value) {
                return;
            }
            this._bsValue = value;
            this.bsValueChange.emit(value);
        },
        enumerable: true,
        configurable: true
    });
    BsDatepickerDirective.prototype.ngOnInit = function () {
        var _this = this;
        this._datepicker.listen({
            outsideClick: this.outsideClick,
            triggers: this.triggers,
            show: function () { return _this.show(); }
        });
        this.setConfig();
    };
    BsDatepickerDirective.prototype.ngOnChanges = function (changes) {
        if (!this._datepickerRef || !this._datepickerRef.instance) {
            return;
        }
        if (changes.minDate) {
            this._datepickerRef.instance.minDate = this.minDate;
        }
        if (changes.maxDate) {
            this._datepickerRef.instance.maxDate = this.maxDate;
        }
        if (changes.isDisabled) {
            this._datepickerRef.instance.isDisabled = this.isDisabled;
        }
    };
    /**
     * Opens an element’s datepicker. This is considered a “manual” triggering of
     * the datepicker.
     */
    /**
       * Opens an element’s datepicker. This is considered a “manual” triggering of
       * the datepicker.
       */
    BsDatepickerDirective.prototype.show = /**
       * Opens an element’s datepicker. This is considered a “manual” triggering of
       * the datepicker.
       */
    function () {
        var _this = this;
        if (this._datepicker.isShown) {
            return;
        }
        this.setConfig();
        this._datepickerRef = this._datepicker
            .provide({ provide: _bs_datepicker_config__WEBPACK_IMPORTED_MODULE_3__["BsDatepickerConfig"], useValue: this._config })
            .attach(_themes_bs_bs_datepicker_container_component__WEBPACK_IMPORTED_MODULE_2__["BsDatepickerContainerComponent"])
            .to(this.container)
            .position({ attachment: this.placement })
            .show({ placement: this.placement });
        // if date changes from external source (model -> view)
        this._subs.push(this.bsValueChange.subscribe(function (value) {
            _this._datepickerRef.instance.value = value;
        }));
        // if date changes from picker (view -> model)
        this._subs.push(this._datepickerRef.instance.valueChange.subscribe(function (value) {
            _this.bsValue = value;
            _this.hide();
        }));
    };
    /**
     * Closes an element’s datepicker. This is considered a “manual” triggering of
     * the datepicker.
     */
    /**
       * Closes an element’s datepicker. This is considered a “manual” triggering of
       * the datepicker.
       */
    BsDatepickerDirective.prototype.hide = /**
       * Closes an element’s datepicker. This is considered a “manual” triggering of
       * the datepicker.
       */
    function () {
        if (this.isOpen) {
            this._datepicker.hide();
        }
        for (var _i = 0, _a = this._subs; _i < _a.length; _i++) {
            var sub = _a[_i];
            sub.unsubscribe();
        }
    };
    /**
     * Toggles an element’s datepicker. This is considered a “manual” triggering
     * of the datepicker.
     */
    /**
       * Toggles an element’s datepicker. This is considered a “manual” triggering
       * of the datepicker.
       */
    BsDatepickerDirective.prototype.toggle = /**
       * Toggles an element’s datepicker. This is considered a “manual” triggering
       * of the datepicker.
       */
    function () {
        if (this.isOpen) {
            return this.hide();
        }
        this.show();
    };
    /**
     * Set config for datepicker
     */
    /**
       * Set config for datepicker
       */
    BsDatepickerDirective.prototype.setConfig = /**
       * Set config for datepicker
       */
    function () {
        this._config = Object.assign({}, this._config, this.bsConfig, {
            value: this._bsValue,
            isDisabled: this.isDisabled,
            minDate: this.minDate || this.bsConfig && this.bsConfig.minDate,
            maxDate: this.maxDate || this.bsConfig && this.bsConfig.maxDate
        });
    };
    BsDatepickerDirective.prototype.ngOnDestroy = function () {
        this._datepicker.dispose();
    };
    BsDatepickerDirective.decorators = [
        { type: _angular_core__WEBPACK_IMPORTED_MODULE_0__["Directive"], args: [{
                    selector: '[bsDatepicker]',
                    exportAs: 'bsDatepicker'
                },] },
    ];
    /** @nocollapse */
    BsDatepickerDirective.ctorParameters = function () { return [
        { type: _bs_datepicker_config__WEBPACK_IMPORTED_MODULE_3__["BsDatepickerConfig"], },
        { type: _angular_core__WEBPACK_IMPORTED_MODULE_0__["ElementRef"], },
        { type: _angular_core__WEBPACK_IMPORTED_MODULE_0__["Renderer2"], },
        { type: _angular_core__WEBPACK_IMPORTED_MODULE_0__["ViewContainerRef"], },
        { type: _component_loader_component_loader_factory__WEBPACK_IMPORTED_MODULE_1__["ComponentLoaderFactory"], },
    ]; };
    BsDatepickerDirective.propDecorators = {
        "placement": [{ type: _angular_core__WEBPACK_IMPORTED_MODULE_0__["Input"] },],
        "triggers": [{ type: _angular_core__WEBPACK_IMPORTED_MODULE_0__["Input"] },],
        "outsideClick": [{ type: _angular_core__WEBPACK_IMPORTED_MODULE_0__["Input"] },],
        "container": [{ type: _angular_core__WEBPACK_IMPORTED_MODULE_0__["Input"] },],
        "isOpen": [{ type: _angular_core__WEBPACK_IMPORTED_MODULE_0__["Input"] },],
        "onShown": [{ type: _angular_core__WEBPACK_IMPORTED_MODULE_0__["Output"] },],
        "onHidden": [{ type: _angular_core__WEBPACK_IMPORTED_MODULE_0__["Output"] },],
        "bsValue": [{ type: _angular_core__WEBPACK_IMPORTED_MODULE_0__["Input"] },],
        "bsConfig": [{ type: _angular_core__WEBPACK_IMPORTED_MODULE_0__["Input"] },],
        "isDisabled": [{ type: _angular_core__WEBPACK_IMPORTED_MODULE_0__["Input"] },],
        "minDate": [{ type: _angular_core__WEBPACK_IMPORTED_MODULE_0__["Input"] },],
        "maxDate": [{ type: _angular_core__WEBPACK_IMPORTED_MODULE_0__["Input"] },],
        "bsValueChange": [{ type: _angular_core__WEBPACK_IMPORTED_MODULE_0__["Output"] },],
    };
    return BsDatepickerDirective;
}());

//# sourceMappingURL=bs-datepicker.component.js.map

/***/ }),

/***/ "./node_modules/ngx-bootstrap/datepicker/bs-datepicker.config.js":
/*!***********************************************************************!*\
  !*** ./node_modules/ngx-bootstrap/datepicker/bs-datepicker.config.js ***!
  \***********************************************************************/
/*! exports provided: BsDatepickerConfig */
/***/ (function(module, __webpack_exports__, __webpack_require__) {

"use strict";
__webpack_require__.r(__webpack_exports__);
/* harmony export (binding) */ __webpack_require__.d(__webpack_exports__, "BsDatepickerConfig", function() { return BsDatepickerConfig; });
/* harmony import */ var _angular_core__WEBPACK_IMPORTED_MODULE_0__ = __webpack_require__(/*! @angular/core */ "./node_modules/@angular/core/fesm5/core.js");

/**
 * For date range picker there are `BsDaterangepickerConfig` which inherits all properties,
 * except `displayMonths`, for range picker it default to `2`
 */
var BsDatepickerConfig = /** @class */ (function () {
    function BsDatepickerConfig() {
        /** CSS class which will be applied to datepicker container,
           * usually used to set color theme
           */
        this.containerClass = 'theme-green';
        // DatepickerRenderOptions
        this.displayMonths = 1;
        /**
           * Allows to hide week numbers in datepicker
           */
        this.showWeekNumbers = true;
        this.dateInputFormat = 'L';
        // range picker
        this.rangeSeparator = ' - ';
        /**
           * Date format for date range input field
           */
        this.rangeInputFormat = 'L';
        // DatepickerFormatOptions
        this.monthTitle = 'MMMM';
        this.yearTitle = 'YYYY';
        this.dayLabel = 'D';
        this.monthLabel = 'MMMM';
        this.yearLabel = 'YYYY';
        this.weekNumbers = 'w';
    }
    BsDatepickerConfig.decorators = [
        { type: _angular_core__WEBPACK_IMPORTED_MODULE_0__["Injectable"] },
    ];
    return BsDatepickerConfig;
}());

//# sourceMappingURL=bs-datepicker.config.js.map

/***/ }),

/***/ "./node_modules/ngx-bootstrap/datepicker/bs-datepicker.module.js":
/*!***********************************************************************!*\
  !*** ./node_modules/ngx-bootstrap/datepicker/bs-datepicker.module.js ***!
  \***********************************************************************/
/*! exports provided: BsDatepickerModule */
/***/ (function(module, __webpack_exports__, __webpack_require__) {

"use strict";
__webpack_require__.r(__webpack_exports__);
/* harmony export (binding) */ __webpack_require__.d(__webpack_exports__, "BsDatepickerModule", function() { return BsDatepickerModule; });
/* harmony import */ var _angular_common__WEBPACK_IMPORTED_MODULE_0__ = __webpack_require__(/*! @angular/common */ "./node_modules/@angular/common/fesm5/common.js");
/* harmony import */ var _angular_core__WEBPACK_IMPORTED_MODULE_1__ = __webpack_require__(/*! @angular/core */ "./node_modules/@angular/core/fesm5/core.js");
/* harmony import */ var _component_loader_component_loader_factory__WEBPACK_IMPORTED_MODULE_2__ = __webpack_require__(/*! ../component-loader/component-loader.factory */ "./node_modules/ngx-bootstrap/component-loader/component-loader.factory.js");
/* harmony import */ var _positioning_positioning_service__WEBPACK_IMPORTED_MODULE_3__ = __webpack_require__(/*! ../positioning/positioning.service */ "./node_modules/ngx-bootstrap/positioning/positioning.service.js");
/* harmony import */ var _utils_warn_once__WEBPACK_IMPORTED_MODULE_4__ = __webpack_require__(/*! ../utils/warn-once */ "./node_modules/ngx-bootstrap/utils/warn-once.js");
/* harmony import */ var _bs_datepicker_input_directive__WEBPACK_IMPORTED_MODULE_5__ = __webpack_require__(/*! ./bs-datepicker-input.directive */ "./node_modules/ngx-bootstrap/datepicker/bs-datepicker-input.directive.js");
/* harmony import */ var _bs_datepicker_component__WEBPACK_IMPORTED_MODULE_6__ = __webpack_require__(/*! ./bs-datepicker.component */ "./node_modules/ngx-bootstrap/datepicker/bs-datepicker.component.js");
/* harmony import */ var _bs_datepicker_config__WEBPACK_IMPORTED_MODULE_7__ = __webpack_require__(/*! ./bs-datepicker.config */ "./node_modules/ngx-bootstrap/datepicker/bs-datepicker.config.js");
/* harmony import */ var _bs_daterangepicker_input_directive__WEBPACK_IMPORTED_MODULE_8__ = __webpack_require__(/*! ./bs-daterangepicker-input.directive */ "./node_modules/ngx-bootstrap/datepicker/bs-daterangepicker-input.directive.js");
/* harmony import */ var _bs_daterangepicker_component__WEBPACK_IMPORTED_MODULE_9__ = __webpack_require__(/*! ./bs-daterangepicker.component */ "./node_modules/ngx-bootstrap/datepicker/bs-daterangepicker.component.js");
/* harmony import */ var _bs_daterangepicker_config__WEBPACK_IMPORTED_MODULE_10__ = __webpack_require__(/*! ./bs-daterangepicker.config */ "./node_modules/ngx-bootstrap/datepicker/bs-daterangepicker.config.js");
/* harmony import */ var _bs_locale_service__WEBPACK_IMPORTED_MODULE_11__ = __webpack_require__(/*! ./bs-locale.service */ "./node_modules/ngx-bootstrap/datepicker/bs-locale.service.js");
/* harmony import */ var _reducer_bs_datepicker_actions__WEBPACK_IMPORTED_MODULE_12__ = __webpack_require__(/*! ./reducer/bs-datepicker.actions */ "./node_modules/ngx-bootstrap/datepicker/reducer/bs-datepicker.actions.js");
/* harmony import */ var _reducer_bs_datepicker_effects__WEBPACK_IMPORTED_MODULE_13__ = __webpack_require__(/*! ./reducer/bs-datepicker.effects */ "./node_modules/ngx-bootstrap/datepicker/reducer/bs-datepicker.effects.js");
/* harmony import */ var _reducer_bs_datepicker_store__WEBPACK_IMPORTED_MODULE_14__ = __webpack_require__(/*! ./reducer/bs-datepicker.store */ "./node_modules/ngx-bootstrap/datepicker/reducer/bs-datepicker.store.js");
/* harmony import */ var _themes_bs_bs_calendar_layout_component__WEBPACK_IMPORTED_MODULE_15__ = __webpack_require__(/*! ./themes/bs/bs-calendar-layout.component */ "./node_modules/ngx-bootstrap/datepicker/themes/bs/bs-calendar-layout.component.js");
/* harmony import */ var _themes_bs_bs_current_date_view_component__WEBPACK_IMPORTED_MODULE_16__ = __webpack_require__(/*! ./themes/bs/bs-current-date-view.component */ "./node_modules/ngx-bootstrap/datepicker/themes/bs/bs-current-date-view.component.js");
/* harmony import */ var _themes_bs_bs_custom_dates_view_component__WEBPACK_IMPORTED_MODULE_17__ = __webpack_require__(/*! ./themes/bs/bs-custom-dates-view.component */ "./node_modules/ngx-bootstrap/datepicker/themes/bs/bs-custom-dates-view.component.js");
/* harmony import */ var _themes_bs_bs_datepicker_container_component__WEBPACK_IMPORTED_MODULE_18__ = __webpack_require__(/*! ./themes/bs/bs-datepicker-container.component */ "./node_modules/ngx-bootstrap/datepicker/themes/bs/bs-datepicker-container.component.js");
/* harmony import */ var _themes_bs_bs_datepicker_day_decorator_directive__WEBPACK_IMPORTED_MODULE_19__ = __webpack_require__(/*! ./themes/bs/bs-datepicker-day-decorator.directive */ "./node_modules/ngx-bootstrap/datepicker/themes/bs/bs-datepicker-day-decorator.directive.js");
/* harmony import */ var _themes_bs_bs_datepicker_navigation_view_component__WEBPACK_IMPORTED_MODULE_20__ = __webpack_require__(/*! ./themes/bs/bs-datepicker-navigation-view.component */ "./node_modules/ngx-bootstrap/datepicker/themes/bs/bs-datepicker-navigation-view.component.js");
/* harmony import */ var _themes_bs_bs_daterangepicker_container_component__WEBPACK_IMPORTED_MODULE_21__ = __webpack_require__(/*! ./themes/bs/bs-daterangepicker-container.component */ "./node_modules/ngx-bootstrap/datepicker/themes/bs/bs-daterangepicker-container.component.js");
/* harmony import */ var _themes_bs_bs_days_calendar_view_component__WEBPACK_IMPORTED_MODULE_22__ = __webpack_require__(/*! ./themes/bs/bs-days-calendar-view.component */ "./node_modules/ngx-bootstrap/datepicker/themes/bs/bs-days-calendar-view.component.js");
/* harmony import */ var _themes_bs_bs_months_calendar_view_component__WEBPACK_IMPORTED_MODULE_23__ = __webpack_require__(/*! ./themes/bs/bs-months-calendar-view.component */ "./node_modules/ngx-bootstrap/datepicker/themes/bs/bs-months-calendar-view.component.js");
/* harmony import */ var _themes_bs_bs_timepicker_view_component__WEBPACK_IMPORTED_MODULE_24__ = __webpack_require__(/*! ./themes/bs/bs-timepicker-view.component */ "./node_modules/ngx-bootstrap/datepicker/themes/bs/bs-timepicker-view.component.js");
/* harmony import */ var _themes_bs_bs_years_calendar_view_component__WEBPACK_IMPORTED_MODULE_25__ = __webpack_require__(/*! ./themes/bs/bs-years-calendar-view.component */ "./node_modules/ngx-bootstrap/datepicker/themes/bs/bs-years-calendar-view.component.js");


























var _exports = [
    _themes_bs_bs_datepicker_container_component__WEBPACK_IMPORTED_MODULE_18__["BsDatepickerContainerComponent"],
    _themes_bs_bs_daterangepicker_container_component__WEBPACK_IMPORTED_MODULE_21__["BsDaterangepickerContainerComponent"],
    _bs_datepicker_component__WEBPACK_IMPORTED_MODULE_6__["BsDatepickerDirective"],
    _bs_datepicker_input_directive__WEBPACK_IMPORTED_MODULE_5__["BsDatepickerInputDirective"],
    _bs_daterangepicker_input_directive__WEBPACK_IMPORTED_MODULE_8__["BsDaterangepickerInputDirective"],
    _bs_daterangepicker_component__WEBPACK_IMPORTED_MODULE_9__["BsDaterangepickerDirective"]
];
var BsDatepickerModule = /** @class */ (function () {
    function BsDatepickerModule() {
        Object(_utils_warn_once__WEBPACK_IMPORTED_MODULE_4__["warnOnce"])("BsDatepickerModule is under development,\n      BREAKING CHANGES are possible,\n      PLEASE, read changelog");
    }
    BsDatepickerModule.forRoot = function () {
        return {
            ngModule: BsDatepickerModule,
            providers: [
                _component_loader_component_loader_factory__WEBPACK_IMPORTED_MODULE_2__["ComponentLoaderFactory"],
                _positioning_positioning_service__WEBPACK_IMPORTED_MODULE_3__["PositioningService"],
                _reducer_bs_datepicker_store__WEBPACK_IMPORTED_MODULE_14__["BsDatepickerStore"],
                _reducer_bs_datepicker_actions__WEBPACK_IMPORTED_MODULE_12__["BsDatepickerActions"],
                _bs_datepicker_config__WEBPACK_IMPORTED_MODULE_7__["BsDatepickerConfig"],
                _bs_daterangepicker_config__WEBPACK_IMPORTED_MODULE_10__["BsDaterangepickerConfig"],
                _reducer_bs_datepicker_effects__WEBPACK_IMPORTED_MODULE_13__["BsDatepickerEffects"],
                _bs_locale_service__WEBPACK_IMPORTED_MODULE_11__["BsLocaleService"]
            ]
        };
    };
    BsDatepickerModule.decorators = [
        { type: _angular_core__WEBPACK_IMPORTED_MODULE_1__["NgModule"], args: [{
                    imports: [_angular_common__WEBPACK_IMPORTED_MODULE_0__["CommonModule"]],
                    declarations: [
                        _themes_bs_bs_datepicker_day_decorator_directive__WEBPACK_IMPORTED_MODULE_19__["BsDatepickerDayDecoratorComponent"],
                        _themes_bs_bs_current_date_view_component__WEBPACK_IMPORTED_MODULE_16__["BsCurrentDateViewComponent"],
                        _themes_bs_bs_datepicker_navigation_view_component__WEBPACK_IMPORTED_MODULE_20__["BsDatepickerNavigationViewComponent"],
                        _themes_bs_bs_timepicker_view_component__WEBPACK_IMPORTED_MODULE_24__["BsTimepickerViewComponent"],
                        _themes_bs_bs_calendar_layout_component__WEBPACK_IMPORTED_MODULE_15__["BsCalendarLayoutComponent"],
                        _themes_bs_bs_days_calendar_view_component__WEBPACK_IMPORTED_MODULE_22__["BsDaysCalendarViewComponent"],
                        _themes_bs_bs_months_calendar_view_component__WEBPACK_IMPORTED_MODULE_23__["BsMonthCalendarViewComponent"],
                        _themes_bs_bs_years_calendar_view_component__WEBPACK_IMPORTED_MODULE_25__["BsYearsCalendarViewComponent"],
                        _themes_bs_bs_custom_dates_view_component__WEBPACK_IMPORTED_MODULE_17__["BsCustomDatesViewComponent"]
                    ].concat(_exports),
                    entryComponents: [
                        _themes_bs_bs_datepicker_container_component__WEBPACK_IMPORTED_MODULE_18__["BsDatepickerContainerComponent"],
                        _themes_bs_bs_daterangepicker_container_component__WEBPACK_IMPORTED_MODULE_21__["BsDaterangepickerContainerComponent"]
                    ],
                    exports: _exports
                },] },
    ];
    /** @nocollapse */
    BsDatepickerModule.ctorParameters = function () { return []; };
    return BsDatepickerModule;
}());

//# sourceMappingURL=bs-datepicker.module.js.map

/***/ }),

/***/ "./node_modules/ngx-bootstrap/datepicker/bs-daterangepicker-input.directive.js":
/*!*************************************************************************************!*\
  !*** ./node_modules/ngx-bootstrap/datepicker/bs-daterangepicker-input.directive.js ***!
  \*************************************************************************************/
/*! exports provided: BsDaterangepickerInputDirective */
/***/ (function(module, __webpack_exports__, __webpack_require__) {

"use strict";
__webpack_require__.r(__webpack_exports__);
/* harmony export (binding) */ __webpack_require__.d(__webpack_exports__, "BsDaterangepickerInputDirective", function() { return BsDaterangepickerInputDirective; });
/* harmony import */ var _angular_core__WEBPACK_IMPORTED_MODULE_0__ = __webpack_require__(/*! @angular/core */ "./node_modules/@angular/core/fesm5/core.js");
/* harmony import */ var _angular_forms__WEBPACK_IMPORTED_MODULE_1__ = __webpack_require__(/*! @angular/forms */ "./node_modules/@angular/forms/fesm5/forms.js");
/* harmony import */ var _chronos_create_local__WEBPACK_IMPORTED_MODULE_2__ = __webpack_require__(/*! ../chronos/create/local */ "./node_modules/ngx-bootstrap/chronos/create/local.js");
/* harmony import */ var _chronos_format__WEBPACK_IMPORTED_MODULE_3__ = __webpack_require__(/*! ../chronos/format */ "./node_modules/ngx-bootstrap/chronos/format.js");
/* harmony import */ var _chronos_locale_locales__WEBPACK_IMPORTED_MODULE_4__ = __webpack_require__(/*! ../chronos/locale/locales */ "./node_modules/ngx-bootstrap/chronos/locale/locales.js");
/* harmony import */ var _chronos_utils_date_compare__WEBPACK_IMPORTED_MODULE_5__ = __webpack_require__(/*! ../chronos/utils/date-compare */ "./node_modules/ngx-bootstrap/chronos/utils/date-compare.js");
/* harmony import */ var _chronos_utils_type_checks__WEBPACK_IMPORTED_MODULE_6__ = __webpack_require__(/*! ../chronos/utils/type-checks */ "./node_modules/ngx-bootstrap/chronos/utils/type-checks.js");
/* harmony import */ var _bs_daterangepicker_component__WEBPACK_IMPORTED_MODULE_7__ = __webpack_require__(/*! ./bs-daterangepicker.component */ "./node_modules/ngx-bootstrap/datepicker/bs-daterangepicker.component.js");
/* harmony import */ var _bs_locale_service__WEBPACK_IMPORTED_MODULE_8__ = __webpack_require__(/*! ./bs-locale.service */ "./node_modules/ngx-bootstrap/datepicker/bs-locale.service.js");









var BS_DATERANGEPICKER_VALUE_ACCESSOR = {
    provide: _angular_forms__WEBPACK_IMPORTED_MODULE_1__["NG_VALUE_ACCESSOR"],
    // tslint:disable-next-line
    useExisting: Object(_angular_core__WEBPACK_IMPORTED_MODULE_0__["forwardRef"])(function () { return BsDaterangepickerInputDirective; }),
    multi: true
};
var BS_DATERANGEPICKER_VALIDATOR = {
    provide: _angular_forms__WEBPACK_IMPORTED_MODULE_1__["NG_VALIDATORS"],
    useExisting: Object(_angular_core__WEBPACK_IMPORTED_MODULE_0__["forwardRef"])(function () { return BsDaterangepickerInputDirective; }),
    multi: true
};
var BsDaterangepickerInputDirective = /** @class */ (function () {
    function BsDaterangepickerInputDirective(_picker, _localeService, _renderer, _elRef, changeDetection) {
        var _this = this;
        this._picker = _picker;
        this._localeService = _localeService;
        this._renderer = _renderer;
        this._elRef = _elRef;
        this.changeDetection = changeDetection;
        this._onChange = Function.prototype;
        this._onTouched = Function.prototype;
        this._validatorChange = Function.prototype;
        // update input value on datepicker value update
        this._picker.bsValueChange.subscribe(function (value) {
            _this._setInputValue(value);
            if (_this._value !== value) {
                _this._value = value;
                _this._onChange(value);
                _this._onTouched();
            }
            _this.changeDetection.markForCheck();
        });
        // update input value on locale change
        this._localeService.localeChange.subscribe(function () {
            _this._setInputValue(_this._value);
        });
    }
    BsDaterangepickerInputDirective.prototype._setInputValue = function (date) {
        var range = '';
        if (date) {
            var start = !date[0] ? ''
                : Object(_chronos_format__WEBPACK_IMPORTED_MODULE_3__["formatDate"])(date[0], this._picker._config.rangeInputFormat, this._localeService.currentLocale);
            var end = !date[1] ? ''
                : Object(_chronos_format__WEBPACK_IMPORTED_MODULE_3__["formatDate"])(date[1], this._picker._config.rangeInputFormat, this._localeService.currentLocale);
            range = (start && end) ? start + this._picker._config.rangeSeparator + end : '';
        }
        this._renderer.setProperty(this._elRef.nativeElement, 'value', range);
    };
    BsDaterangepickerInputDirective.prototype.onChange = function (event) {
        this.writeValue(event.target.value);
        this._onChange(this._value);
        this._onTouched();
    };
    BsDaterangepickerInputDirective.prototype.validate = function (c) {
        var _value = c.value;
        if (_value === null || _value === undefined || !Object(_chronos_utils_type_checks__WEBPACK_IMPORTED_MODULE_6__["isArray"])(_value)) {
            return null;
        }
        var _isDateValid = Object(_chronos_utils_type_checks__WEBPACK_IMPORTED_MODULE_6__["isDateValid"])(_value[0]) && Object(_chronos_utils_type_checks__WEBPACK_IMPORTED_MODULE_6__["isDateValid"])(_value[0]);
        if (!_isDateValid) {
            return { bsDate: { invalid: _value } };
        }
        if (this._picker && this._picker.minDate && Object(_chronos_utils_date_compare__WEBPACK_IMPORTED_MODULE_5__["isBefore"])(_value[0], this._picker.minDate, 'date')) {
            return { bsDate: { minDate: this._picker.minDate } };
        }
        if (this._picker && this._picker.maxDate && Object(_chronos_utils_date_compare__WEBPACK_IMPORTED_MODULE_5__["isAfter"])(_value[1], this._picker.maxDate, 'date')) {
            return { bsDate: { maxDate: this._picker.maxDate } };
        }
    };
    BsDaterangepickerInputDirective.prototype.registerOnValidatorChange = function (fn) {
        this._validatorChange = fn;
    };
    BsDaterangepickerInputDirective.prototype.writeValue = function (value) {
        var _this = this;
        if (!value) {
            this._value = null;
        }
        else {
            var _localeKey = this._localeService.currentLocale;
            var _locale = Object(_chronos_locale_locales__WEBPACK_IMPORTED_MODULE_4__["getLocale"])(_localeKey);
            if (!_locale) {
                throw new Error("Locale \"" + _localeKey + "\" is not defined, please add it with \"defineLocale(...)\"");
            }
            var _input = [];
            if (typeof value === 'string') {
                _input = value.split(this._picker._config.rangeSeparator);
            }
            if (Array.isArray(value)) {
                _input = value;
            }
            this._value = _input
                .map(function (_val) {
                return Object(_chronos_create_local__WEBPACK_IMPORTED_MODULE_2__["parseDate"])(_val, _this._picker._config.dateInputFormat, _this._localeService.currentLocale);
            })
                .map(function (date) { return (isNaN(date.valueOf()) ? null : date); });
        }
        this._picker.bsValue = this._value;
    };
    BsDaterangepickerInputDirective.prototype.setDisabledState = function (isDisabled) {
        this._picker.isDisabled = isDisabled;
        if (isDisabled) {
            this._renderer.setAttribute(this._elRef.nativeElement, 'disabled', 'disabled');
            return;
        }
        this._renderer.removeAttribute(this._elRef.nativeElement, 'disabled');
    };
    BsDaterangepickerInputDirective.prototype.registerOnChange = function (fn) {
        this._onChange = fn;
    };
    BsDaterangepickerInputDirective.prototype.registerOnTouched = function (fn) {
        this._onTouched = fn;
    };
    BsDaterangepickerInputDirective.prototype.onBlur = function () {
        this._onTouched();
    };
    BsDaterangepickerInputDirective.prototype.hide = function () {
        this._picker.hide();
    };
    BsDaterangepickerInputDirective.decorators = [
        { type: _angular_core__WEBPACK_IMPORTED_MODULE_0__["Directive"], args: [{
                    selector: "input[bsDaterangepicker]",
                    host: {
                        '(change)': 'onChange($event)',
                        '(keyup.esc)': 'hide()',
                        '(blur)': 'onBlur()'
                    },
                    providers: [BS_DATERANGEPICKER_VALUE_ACCESSOR, BS_DATERANGEPICKER_VALIDATOR]
                },] },
    ];
    /** @nocollapse */
    BsDaterangepickerInputDirective.ctorParameters = function () { return [
        { type: _bs_daterangepicker_component__WEBPACK_IMPORTED_MODULE_7__["BsDaterangepickerDirective"], decorators: [{ type: _angular_core__WEBPACK_IMPORTED_MODULE_0__["Host"] },] },
        { type: _bs_locale_service__WEBPACK_IMPORTED_MODULE_8__["BsLocaleService"], },
        { type: _angular_core__WEBPACK_IMPORTED_MODULE_0__["Renderer2"], },
        { type: _angular_core__WEBPACK_IMPORTED_MODULE_0__["ElementRef"], },
        { type: _angular_core__WEBPACK_IMPORTED_MODULE_0__["ChangeDetectorRef"], },
    ]; };
    return BsDaterangepickerInputDirective;
}());

//# sourceMappingURL=bs-daterangepicker-input.directive.js.map

/***/ }),

/***/ "./node_modules/ngx-bootstrap/datepicker/bs-daterangepicker.component.js":
/*!*******************************************************************************!*\
  !*** ./node_modules/ngx-bootstrap/datepicker/bs-daterangepicker.component.js ***!
  \*******************************************************************************/
/*! exports provided: BsDaterangepickerDirective */
/***/ (function(module, __webpack_exports__, __webpack_require__) {

"use strict";
__webpack_require__.r(__webpack_exports__);
/* harmony export (binding) */ __webpack_require__.d(__webpack_exports__, "BsDaterangepickerDirective", function() { return BsDaterangepickerDirective; });
/* harmony import */ var _angular_core__WEBPACK_IMPORTED_MODULE_0__ = __webpack_require__(/*! @angular/core */ "./node_modules/@angular/core/fesm5/core.js");
/* harmony import */ var _bs_daterangepicker_config__WEBPACK_IMPORTED_MODULE_1__ = __webpack_require__(/*! ./bs-daterangepicker.config */ "./node_modules/ngx-bootstrap/datepicker/bs-daterangepicker.config.js");
/* harmony import */ var _themes_bs_bs_daterangepicker_container_component__WEBPACK_IMPORTED_MODULE_2__ = __webpack_require__(/*! ./themes/bs/bs-daterangepicker-container.component */ "./node_modules/ngx-bootstrap/datepicker/themes/bs/bs-daterangepicker-container.component.js");
/* harmony import */ var _component_loader_component_loader_factory__WEBPACK_IMPORTED_MODULE_3__ = __webpack_require__(/*! ../component-loader/component-loader.factory */ "./node_modules/ngx-bootstrap/component-loader/component-loader.factory.js");
/* harmony import */ var _bs_datepicker_config__WEBPACK_IMPORTED_MODULE_4__ = __webpack_require__(/*! ./bs-datepicker.config */ "./node_modules/ngx-bootstrap/datepicker/bs-datepicker.config.js");
/* harmony import */ var rxjs_operators__WEBPACK_IMPORTED_MODULE_5__ = __webpack_require__(/*! rxjs/operators */ "./node_modules/rxjs/_esm5/operators/index.js");






var BsDaterangepickerDirective = /** @class */ (function () {
    function BsDaterangepickerDirective(_config, _elementRef, _renderer, _viewContainerRef, cis) {
        this._config = _config;
        /**
           * Placement of a daterangepicker. Accepts: "top", "bottom", "left", "right"
           */
        this.placement = 'bottom';
        /**
           * Specifies events that should trigger. Supports a space separated list of
           * event names.
           */
        this.triggers = 'click';
        /**
           * Close daterangepicker on outside click
           */
        this.outsideClick = true;
        /**
           * A selector specifying the element the daterangepicker should be appended
           * to. Currently only supports "body".
           */
        this.container = 'body';
        /**
           * Emits when daterangepicker value has been changed
           */
        this.bsValueChange = new _angular_core__WEBPACK_IMPORTED_MODULE_0__["EventEmitter"]();
        this._subs = [];
        this._datepicker = cis.createLoader(_elementRef, _viewContainerRef, _renderer);
        Object.assign(this, _config);
        this.onShown = this._datepicker.onShown;
        this.onHidden = this._datepicker.onHidden;
    }
    Object.defineProperty(BsDaterangepickerDirective.prototype, "isOpen", {
        get: /**
           * Returns whether or not the daterangepicker is currently being shown
           */
        function () {
            return this._datepicker.isShown;
        },
        set: function (value) {
            if (value) {
                this.show();
            }
            else {
                this.hide();
            }
        },
        enumerable: true,
        configurable: true
    });
    Object.defineProperty(BsDaterangepickerDirective.prototype, "bsValue", {
        set: /**
           * Initial value of daterangepicker
           */
        function (value) {
            if (this._bsValue === value) {
                return;
            }
            this._bsValue = value;
            this.bsValueChange.emit(value);
        },
        enumerable: true,
        configurable: true
    });
    BsDaterangepickerDirective.prototype.ngOnInit = function () {
        var _this = this;
        this._datepicker.listen({
            outsideClick: this.outsideClick,
            triggers: this.triggers,
            show: function () { return _this.show(); }
        });
        this.setConfig();
    };
    BsDaterangepickerDirective.prototype.ngOnChanges = function (changes) {
        if (!this._datepickerRef || !this._datepickerRef.instance) {
            return;
        }
        if (changes.minDate) {
            this._datepickerRef.instance.minDate = this.minDate;
        }
        if (changes.maxDate) {
            this._datepickerRef.instance.maxDate = this.maxDate;
        }
        if (changes.isDisabled) {
            this._datepickerRef.instance.isDisabled = this.isDisabled;
        }
    };
    /**
     * Opens an element’s datepicker. This is considered a “manual” triggering of
     * the datepicker.
     */
    /**
       * Opens an element’s datepicker. This is considered a “manual” triggering of
       * the datepicker.
       */
    BsDaterangepickerDirective.prototype.show = /**
       * Opens an element’s datepicker. This is considered a “manual” triggering of
       * the datepicker.
       */
    function () {
        var _this = this;
        if (this._datepicker.isShown) {
            return;
        }
        this.setConfig();
        this._datepickerRef = this._datepicker
            .provide({ provide: _bs_datepicker_config__WEBPACK_IMPORTED_MODULE_4__["BsDatepickerConfig"], useValue: this._config })
            .attach(_themes_bs_bs_daterangepicker_container_component__WEBPACK_IMPORTED_MODULE_2__["BsDaterangepickerContainerComponent"])
            .to(this.container)
            .position({ attachment: this.placement })
            .show({ placement: this.placement });
        // if date changes from external source (model -> view)
        this._subs.push(this.bsValueChange.subscribe(function (value) {
            _this._datepickerRef.instance.value = value;
        }));
        // if date changes from picker (view -> model)
        this._subs.push(this._datepickerRef.instance.valueChange
            .pipe(Object(rxjs_operators__WEBPACK_IMPORTED_MODULE_5__["filter"])(function (range) { return range && range[0] && !!range[1]; }))
            .subscribe(function (value) {
            _this.bsValue = value;
            _this.hide();
        }));
    };
    /**
     * Set config for daterangepicker
     */
    /**
       * Set config for daterangepicker
       */
    BsDaterangepickerDirective.prototype.setConfig = /**
       * Set config for daterangepicker
       */
    function () {
        this._config = Object.assign({}, this._config, this.bsConfig, {
            value: this._bsValue,
            isDisabled: this.isDisabled,
            minDate: this.minDate || this.bsConfig && this.bsConfig.minDate,
            maxDate: this.maxDate || this.bsConfig && this.bsConfig.maxDate
        });
    };
    /**
     * Closes an element’s datepicker. This is considered a “manual” triggering of
     * the datepicker.
     */
    /**
       * Closes an element’s datepicker. This is considered a “manual” triggering of
       * the datepicker.
       */
    BsDaterangepickerDirective.prototype.hide = /**
       * Closes an element’s datepicker. This is considered a “manual” triggering of
       * the datepicker.
       */
    function () {
        if (this.isOpen) {
            this._datepicker.hide();
        }
        for (var _i = 0, _a = this._subs; _i < _a.length; _i++) {
            var sub = _a[_i];
            sub.unsubscribe();
        }
    };
    /**
     * Toggles an element’s datepicker. This is considered a “manual” triggering
     * of the datepicker.
     */
    /**
       * Toggles an element’s datepicker. This is considered a “manual” triggering
       * of the datepicker.
       */
    BsDaterangepickerDirective.prototype.toggle = /**
       * Toggles an element’s datepicker. This is considered a “manual” triggering
       * of the datepicker.
       */
    function () {
        if (this.isOpen) {
            return this.hide();
        }
        this.show();
    };
    BsDaterangepickerDirective.prototype.ngOnDestroy = function () {
        this._datepicker.dispose();
    };
    BsDaterangepickerDirective.decorators = [
        { type: _angular_core__WEBPACK_IMPORTED_MODULE_0__["Directive"], args: [{
                    selector: '[bsDaterangepicker]',
                    exportAs: 'bsDaterangepicker'
                },] },
    ];
    /** @nocollapse */
    BsDaterangepickerDirective.ctorParameters = function () { return [
        { type: _bs_daterangepicker_config__WEBPACK_IMPORTED_MODULE_1__["BsDaterangepickerConfig"], },
        { type: _angular_core__WEBPACK_IMPORTED_MODULE_0__["ElementRef"], },
        { type: _angular_core__WEBPACK_IMPORTED_MODULE_0__["Renderer2"], },
        { type: _angular_core__WEBPACK_IMPORTED_MODULE_0__["ViewContainerRef"], },
        { type: _component_loader_component_loader_factory__WEBPACK_IMPORTED_MODULE_3__["ComponentLoaderFactory"], },
    ]; };
    BsDaterangepickerDirective.propDecorators = {
        "placement": [{ type: _angular_core__WEBPACK_IMPORTED_MODULE_0__["Input"] },],
        "triggers": [{ type: _angular_core__WEBPACK_IMPORTED_MODULE_0__["Input"] },],
        "outsideClick": [{ type: _angular_core__WEBPACK_IMPORTED_MODULE_0__["Input"] },],
        "container": [{ type: _angular_core__WEBPACK_IMPORTED_MODULE_0__["Input"] },],
        "isOpen": [{ type: _angular_core__WEBPACK_IMPORTED_MODULE_0__["Input"] },],
        "onShown": [{ type: _angular_core__WEBPACK_IMPORTED_MODULE_0__["Output"] },],
        "onHidden": [{ type: _angular_core__WEBPACK_IMPORTED_MODULE_0__["Output"] },],
        "bsValue": [{ type: _angular_core__WEBPACK_IMPORTED_MODULE_0__["Input"] },],
        "bsConfig": [{ type: _angular_core__WEBPACK_IMPORTED_MODULE_0__["Input"] },],
        "isDisabled": [{ type: _angular_core__WEBPACK_IMPORTED_MODULE_0__["Input"] },],
        "minDate": [{ type: _angular_core__WEBPACK_IMPORTED_MODULE_0__["Input"] },],
        "maxDate": [{ type: _angular_core__WEBPACK_IMPORTED_MODULE_0__["Input"] },],
        "bsValueChange": [{ type: _angular_core__WEBPACK_IMPORTED_MODULE_0__["Output"] },],
    };
    return BsDaterangepickerDirective;
}());

//# sourceMappingURL=bs-daterangepicker.component.js.map

/***/ }),

/***/ "./node_modules/ngx-bootstrap/datepicker/bs-daterangepicker.config.js":
/*!****************************************************************************!*\
  !*** ./node_modules/ngx-bootstrap/datepicker/bs-daterangepicker.config.js ***!
  \****************************************************************************/
/*! exports provided: BsDaterangepickerConfig */
/***/ (function(module, __webpack_exports__, __webpack_require__) {

"use strict";
__webpack_require__.r(__webpack_exports__);
/* harmony export (binding) */ __webpack_require__.d(__webpack_exports__, "BsDaterangepickerConfig", function() { return BsDaterangepickerConfig; });
/* harmony import */ var _angular_core__WEBPACK_IMPORTED_MODULE_0__ = __webpack_require__(/*! @angular/core */ "./node_modules/@angular/core/fesm5/core.js");
/* harmony import */ var _bs_datepicker_config__WEBPACK_IMPORTED_MODULE_1__ = __webpack_require__(/*! ./bs-datepicker.config */ "./node_modules/ngx-bootstrap/datepicker/bs-datepicker.config.js");
var __extends = (undefined && undefined.__extends) || (function () {
    var extendStatics = Object.setPrototypeOf ||
        ({ __proto__: [] } instanceof Array && function (d, b) { d.__proto__ = b; }) ||
        function (d, b) { for (var p in b) if (b.hasOwnProperty(p)) d[p] = b[p]; };
    return function (d, b) {
        extendStatics(d, b);
        function __() { this.constructor = d; }
        d.prototype = b === null ? Object.create(b) : (__.prototype = b.prototype, new __());
    };
})();


var BsDaterangepickerConfig = /** @class */ (function (_super) {
    __extends(BsDaterangepickerConfig, _super);
    function BsDaterangepickerConfig() {
        var _this = _super !== null && _super.apply(this, arguments) || this;
        // DatepickerRenderOptions
        _this.displayMonths = 2;
        return _this;
    }
    BsDaterangepickerConfig.decorators = [
        { type: _angular_core__WEBPACK_IMPORTED_MODULE_0__["Injectable"] },
    ];
    return BsDaterangepickerConfig;
}(_bs_datepicker_config__WEBPACK_IMPORTED_MODULE_1__["BsDatepickerConfig"]));

//# sourceMappingURL=bs-daterangepicker.config.js.map

/***/ }),

/***/ "./node_modules/ngx-bootstrap/datepicker/bs-locale.service.js":
/*!********************************************************************!*\
  !*** ./node_modules/ngx-bootstrap/datepicker/bs-locale.service.js ***!
  \********************************************************************/
/*! exports provided: BsLocaleService */
/***/ (function(module, __webpack_exports__, __webpack_require__) {

"use strict";
__webpack_require__.r(__webpack_exports__);
/* harmony export (binding) */ __webpack_require__.d(__webpack_exports__, "BsLocaleService", function() { return BsLocaleService; });
/* harmony import */ var _angular_core__WEBPACK_IMPORTED_MODULE_0__ = __webpack_require__(/*! @angular/core */ "./node_modules/@angular/core/fesm5/core.js");
/* harmony import */ var rxjs__WEBPACK_IMPORTED_MODULE_1__ = __webpack_require__(/*! rxjs */ "./node_modules/rxjs/_esm5/index.js");


var BsLocaleService = /** @class */ (function () {
    function BsLocaleService() {
        this._defaultLocale = 'en';
        this._locale = new rxjs__WEBPACK_IMPORTED_MODULE_1__["BehaviorSubject"](this._defaultLocale);
        this._localeChange = this._locale.asObservable();
    }
    Object.defineProperty(BsLocaleService.prototype, "locale", {
        get: function () {
            return this._locale;
        },
        enumerable: true,
        configurable: true
    });
    Object.defineProperty(BsLocaleService.prototype, "localeChange", {
        get: function () {
            return this._localeChange;
        },
        enumerable: true,
        configurable: true
    });
    Object.defineProperty(BsLocaleService.prototype, "currentLocale", {
        get: function () {
            return this._locale.getValue();
        },
        enumerable: true,
        configurable: true
    });
    BsLocaleService.prototype.use = function (locale) {
        if (locale === this.currentLocale) {
            return;
        }
        this._locale.next(locale);
    };
    BsLocaleService.decorators = [
        { type: _angular_core__WEBPACK_IMPORTED_MODULE_0__["Injectable"] },
    ];
    return BsLocaleService;
}());

//# sourceMappingURL=bs-locale.service.js.map

/***/ }),

/***/ "./node_modules/ngx-bootstrap/datepicker/date-formatter.js":
/*!*****************************************************************!*\
  !*** ./node_modules/ngx-bootstrap/datepicker/date-formatter.js ***!
  \*****************************************************************/
/*! exports provided: DateFormatter */
/***/ (function(module, __webpack_exports__, __webpack_require__) {

"use strict";
__webpack_require__.r(__webpack_exports__);
/* harmony export (binding) */ __webpack_require__.d(__webpack_exports__, "DateFormatter", function() { return DateFormatter; });
/* harmony import */ var _chronos_format__WEBPACK_IMPORTED_MODULE_0__ = __webpack_require__(/*! ../chronos/format */ "./node_modules/ngx-bootstrap/chronos/format.js");

var DateFormatter = /** @class */ (function () {
    function DateFormatter() {
    }
    DateFormatter.prototype.format = function (date, format, locale) {
        return Object(_chronos_format__WEBPACK_IMPORTED_MODULE_0__["formatDate"])(date, format, locale);
    };
    return DateFormatter;
}());

//# sourceMappingURL=date-formatter.js.map

/***/ }),

/***/ "./node_modules/ngx-bootstrap/datepicker/datepicker-inner.component.js":
/*!*****************************************************************************!*\
  !*** ./node_modules/ngx-bootstrap/datepicker/datepicker-inner.component.js ***!
  \*****************************************************************************/
/*! exports provided: DatePickerInnerComponent */
/***/ (function(module, __webpack_exports__, __webpack_require__) {

"use strict";
__webpack_require__.r(__webpack_exports__);
/* harmony export (binding) */ __webpack_require__.d(__webpack_exports__, "DatePickerInnerComponent", function() { return DatePickerInnerComponent; });
/* harmony import */ var _angular_core__WEBPACK_IMPORTED_MODULE_0__ = __webpack_require__(/*! @angular/core */ "./node_modules/@angular/core/fesm5/core.js");
/* harmony import */ var _date_formatter__WEBPACK_IMPORTED_MODULE_1__ = __webpack_require__(/*! ./date-formatter */ "./node_modules/ngx-bootstrap/datepicker/date-formatter.js");


// const MIN_DATE:Date = void 0;
// const MAX_DATE:Date = void 0;
// const DAYS_IN_MONTH = [31, 28, 31, 30, 31, 30, 31, 31, 30, 31, 30, 31];
/*
 const KEYS = {
 13: 'enter',
 32: 'space',
 33: 'pageup',
 34: 'pagedown',
 35: 'end',
 36: 'home',
 37: 'left',
 38: 'up',
 39: 'right',
 40: 'down'
 };
 */
var DatePickerInnerComponent = /** @class */ (function () {
    function DatePickerInnerComponent() {
        this.selectionDone = new _angular_core__WEBPACK_IMPORTED_MODULE_0__["EventEmitter"](undefined);
        this.update = new _angular_core__WEBPACK_IMPORTED_MODULE_0__["EventEmitter"](false);
        this.activeDateChange = new _angular_core__WEBPACK_IMPORTED_MODULE_0__["EventEmitter"](undefined);
        this.stepDay = {};
        this.stepMonth = {};
        this.stepYear = {};
        this.modes = ['day', 'month', 'year'];
        this.dateFormatter = new _date_formatter__WEBPACK_IMPORTED_MODULE_1__["DateFormatter"]();
    }
    Object.defineProperty(DatePickerInnerComponent.prototype, "activeDate", {
        get: function () {
            return this._activeDate;
        },
        set: function (value) {
            this._activeDate = value;
        },
        enumerable: true,
        configurable: true
    });
    // todo: add formatter value to Date object
    // todo: add formatter value to Date object
    DatePickerInnerComponent.prototype.ngOnInit = 
    // todo: add formatter value to Date object
    function () {
        // todo: use date for unique value
        this.uniqueId = "datepicker--" + Math.floor(Math.random() * 10000);
        if (this.initDate) {
            this.activeDate = this.initDate;
            this.selectedDate = new Date(this.activeDate.valueOf());
            this.update.emit(this.activeDate);
        }
        else if (this.activeDate === undefined) {
            this.activeDate = new Date();
        }
    };
    // this.refreshView should be called here to reflect the changes on the fly
    // tslint:disable-next-line:no-unused-variable
    // this.refreshView should be called here to reflect the changes on the fly
    // tslint:disable-next-line:no-unused-variable
    DatePickerInnerComponent.prototype.ngOnChanges = 
    // this.refreshView should be called here to reflect the changes on the fly
    // tslint:disable-next-line:no-unused-variable
    function (changes) {
        this.refreshView();
        this.checkIfActiveDateGotUpdated(changes.activeDate);
    };
    // Check if activeDate has been update and then emit the activeDateChange with the new date
    // Check if activeDate has been update and then emit the activeDateChange with the new date
    DatePickerInnerComponent.prototype.checkIfActiveDateGotUpdated = 
    // Check if activeDate has been update and then emit the activeDateChange with the new date
    function (activeDate) {
        if (activeDate && !activeDate.firstChange) {
            var previousValue = activeDate.previousValue;
            if (previousValue &&
                previousValue instanceof Date &&
                previousValue.getTime() !== activeDate.currentValue.getTime()) {
                this.activeDateChange.emit(this.activeDate);
            }
        }
    };
    DatePickerInnerComponent.prototype.setCompareHandler = function (handler, type) {
        if (type === 'day') {
            this.compareHandlerDay = handler;
        }
        if (type === 'month') {
            this.compareHandlerMonth = handler;
        }
        if (type === 'year') {
            this.compareHandlerYear = handler;
        }
    };
    DatePickerInnerComponent.prototype.compare = function (date1, date2) {
        if (date1 === undefined || date2 === undefined) {
            return undefined;
        }
        if (this.datepickerMode === 'day' && this.compareHandlerDay) {
            return this.compareHandlerDay(date1, date2);
        }
        if (this.datepickerMode === 'month' && this.compareHandlerMonth) {
            return this.compareHandlerMonth(date1, date2);
        }
        if (this.datepickerMode === 'year' && this.compareHandlerYear) {
            return this.compareHandlerYear(date1, date2);
        }
        return void 0;
    };
    DatePickerInnerComponent.prototype.setRefreshViewHandler = function (handler, type) {
        if (type === 'day') {
            this.refreshViewHandlerDay = handler;
        }
        if (type === 'month') {
            this.refreshViewHandlerMonth = handler;
        }
        if (type === 'year') {
            this.refreshViewHandlerYear = handler;
        }
    };
    DatePickerInnerComponent.prototype.refreshView = function () {
        if (this.datepickerMode === 'day' && this.refreshViewHandlerDay) {
            this.refreshViewHandlerDay();
        }
        if (this.datepickerMode === 'month' && this.refreshViewHandlerMonth) {
            this.refreshViewHandlerMonth();
        }
        if (this.datepickerMode === 'year' && this.refreshViewHandlerYear) {
            this.refreshViewHandlerYear();
        }
    };
    DatePickerInnerComponent.prototype.dateFilter = function (date, format) {
        return this.dateFormatter.format(date, format, this.locale);
    };
    DatePickerInnerComponent.prototype.isActive = function (dateObject) {
        if (this.compare(dateObject.date, this.activeDate) === 0) {
            this.activeDateId = dateObject.uid;
            return true;
        }
        return false;
    };
    DatePickerInnerComponent.prototype.createDateObject = function (date, format) {
        var dateObject = {};
        dateObject.date = new Date(date.getFullYear(), date.getMonth(), date.getDate());
        dateObject.date = this.fixTimeZone(dateObject.date);
        dateObject.label = this.dateFilter(date, format);
        dateObject.selected = this.compare(date, this.selectedDate) === 0;
        dateObject.disabled = this.isDisabled(date);
        dateObject.current = this.compare(date, new Date()) === 0;
        dateObject.customClass = this.getCustomClassForDate(dateObject.date);
        return dateObject;
    };
    DatePickerInnerComponent.prototype.split = function (arr, size) {
        var arrays = [];
        while (arr.length > 0) {
            arrays.push(arr.splice(0, size));
        }
        return arrays;
    };
    // Fix a hard-reproducible bug with timezones
    // The bug depends on OS, browser, current timezone and current date
    // i.e.
    // var date = new Date(2014, 0, 1);
    // console.log(date.getFullYear(), date.getMonth(), date.getDate(),
    // date.getHours()); can result in "2013 11 31 23" because of the bug.
    // Fix a hard-reproducible bug with timezones
    // The bug depends on OS, browser, current timezone and current date
    // i.e.
    // var date = new Date(2014, 0, 1);
    // console.log(date.getFullYear(), date.getMonth(), date.getDate(),
    // date.getHours()); can result in "2013 11 31 23" because of the bug.
    DatePickerInnerComponent.prototype.fixTimeZone = 
    // Fix a hard-reproducible bug with timezones
    // The bug depends on OS, browser, current timezone and current date
    // i.e.
    // var date = new Date(2014, 0, 1);
    // console.log(date.getFullYear(), date.getMonth(), date.getDate(),
    // date.getHours()); can result in "2013 11 31 23" because of the bug.
    function (date) {
        var hours = date.getHours();
        return new Date(date.getFullYear(), date.getMonth(), date.getDate(), hours === 23 ? hours + 2 : 0);
    };
    DatePickerInnerComponent.prototype.select = function (date, isManual) {
        if (isManual === void 0) { isManual = true; }
        if (this.datepickerMode === this.minMode) {
            if (!this.activeDate) {
                this.activeDate = new Date(0, 0, 0, 0, 0, 0, 0);
            }
            this.activeDate = new Date(date.getFullYear(), date.getMonth(), date.getDate());
            this.activeDate = this.fixTimeZone(this.activeDate);
            if (isManual) {
                this.selectionDone.emit(this.activeDate);
            }
        }
        else {
            this.activeDate = new Date(date.getFullYear(), date.getMonth(), date.getDate());
            this.activeDate = this.fixTimeZone(this.activeDate);
            if (isManual) {
                this.datepickerMode = this.modes[this.modes.indexOf(this.datepickerMode) - 1];
            }
        }
        this.selectedDate = new Date(this.activeDate.valueOf());
        this.update.emit(this.activeDate);
        this.refreshView();
    };
    DatePickerInnerComponent.prototype.move = function (direction) {
        var expectedStep;
        if (this.datepickerMode === 'day') {
            expectedStep = this.stepDay;
        }
        if (this.datepickerMode === 'month') {
            expectedStep = this.stepMonth;
        }
        if (this.datepickerMode === 'year') {
            expectedStep = this.stepYear;
        }
        if (expectedStep) {
            var year = this.activeDate.getFullYear() + direction * (expectedStep.years || 0);
            var month = this.activeDate.getMonth() + direction * (expectedStep.months || 0);
            this.activeDate = new Date(year, month, 1);
            this.refreshView();
            this.activeDateChange.emit(this.activeDate);
        }
    };
    DatePickerInnerComponent.prototype.toggleMode = function (_direction) {
        var direction = _direction || 1;
        if ((this.datepickerMode === this.maxMode && direction === 1) ||
            (this.datepickerMode === this.minMode && direction === -1)) {
            return;
        }
        this.datepickerMode = this.modes[this.modes.indexOf(this.datepickerMode) + direction];
        this.refreshView();
    };
    DatePickerInnerComponent.prototype.getCustomClassForDate = function (date) {
        var _this = this;
        if (!this.customClass) {
            return '';
        }
        // todo: build a hash of custom classes, it will work faster
        var customClassObject = this.customClass.find(function (customClass) {
            return (customClass.date.valueOf() === date.valueOf() &&
                customClass.mode === _this.datepickerMode);
        }, this);
        return customClassObject === undefined ? '' : customClassObject.clazz;
    };
    DatePickerInnerComponent.prototype.compareDateDisabled = function (date1Disabled, date2) {
        if (date1Disabled === undefined || date2 === undefined) {
            return undefined;
        }
        if (date1Disabled.mode === 'day' && this.compareHandlerDay) {
            return this.compareHandlerDay(date1Disabled.date, date2);
        }
        if (date1Disabled.mode === 'month' && this.compareHandlerMonth) {
            return this.compareHandlerMonth(date1Disabled.date, date2);
        }
        if (date1Disabled.mode === 'year' && this.compareHandlerYear) {
            return this.compareHandlerYear(date1Disabled.date, date2);
        }
        return undefined;
    };
    DatePickerInnerComponent.prototype.isDisabled = function (date) {
        var _this = this;
        var isDateDisabled = false;
        if (this.dateDisabled) {
            this.dateDisabled.forEach(function (disabledDate) {
                if (_this.compareDateDisabled(disabledDate, date) === 0) {
                    isDateDisabled = true;
                }
            });
        }
        if (this.dayDisabled) {
            isDateDisabled =
                isDateDisabled ||
                    this.dayDisabled.indexOf(date.getDay()) > -1;
        }
        return (isDateDisabled ||
            (this.minDate && this.compare(date, this.minDate) < 0) ||
            (this.maxDate && this.compare(date, this.maxDate) > 0));
    };
    DatePickerInnerComponent.decorators = [
        { type: _angular_core__WEBPACK_IMPORTED_MODULE_0__["Component"], args: [{
                    selector: 'datepicker-inner',
                    template: "\n    <!--&lt;!&ndash;ng-keydown=\"keydown($event)\"&ndash;&gt;-->\n    <div *ngIf=\"datepickerMode\" class=\"well well-sm bg-faded p-a card\" role=\"application\" >\n      <ng-content></ng-content>\n    </div>\n  "
                },] },
    ];
    /** @nocollapse */
    DatePickerInnerComponent.propDecorators = {
        "locale": [{ type: _angular_core__WEBPACK_IMPORTED_MODULE_0__["Input"] },],
        "datepickerMode": [{ type: _angular_core__WEBPACK_IMPORTED_MODULE_0__["Input"] },],
        "startingDay": [{ type: _angular_core__WEBPACK_IMPORTED_MODULE_0__["Input"] },],
        "yearRange": [{ type: _angular_core__WEBPACK_IMPORTED_MODULE_0__["Input"] },],
        "minDate": [{ type: _angular_core__WEBPACK_IMPORTED_MODULE_0__["Input"] },],
        "maxDate": [{ type: _angular_core__WEBPACK_IMPORTED_MODULE_0__["Input"] },],
        "minMode": [{ type: _angular_core__WEBPACK_IMPORTED_MODULE_0__["Input"] },],
        "maxMode": [{ type: _angular_core__WEBPACK_IMPORTED_MODULE_0__["Input"] },],
        "showWeeks": [{ type: _angular_core__WEBPACK_IMPORTED_MODULE_0__["Input"] },],
        "formatDay": [{ type: _angular_core__WEBPACK_IMPORTED_MODULE_0__["Input"] },],
        "formatMonth": [{ type: _angular_core__WEBPACK_IMPORTED_MODULE_0__["Input"] },],
        "formatYear": [{ type: _angular_core__WEBPACK_IMPORTED_MODULE_0__["Input"] },],
        "formatDayHeader": [{ type: _angular_core__WEBPACK_IMPORTED_MODULE_0__["Input"] },],
        "formatDayTitle": [{ type: _angular_core__WEBPACK_IMPORTED_MODULE_0__["Input"] },],
        "formatMonthTitle": [{ type: _angular_core__WEBPACK_IMPORTED_MODULE_0__["Input"] },],
        "onlyCurrentMonth": [{ type: _angular_core__WEBPACK_IMPORTED_MODULE_0__["Input"] },],
        "shortcutPropagation": [{ type: _angular_core__WEBPACK_IMPORTED_MODULE_0__["Input"] },],
        "customClass": [{ type: _angular_core__WEBPACK_IMPORTED_MODULE_0__["Input"] },],
        "monthColLimit": [{ type: _angular_core__WEBPACK_IMPORTED_MODULE_0__["Input"] },],
        "yearColLimit": [{ type: _angular_core__WEBPACK_IMPORTED_MODULE_0__["Input"] },],
        "dateDisabled": [{ type: _angular_core__WEBPACK_IMPORTED_MODULE_0__["Input"] },],
        "dayDisabled": [{ type: _angular_core__WEBPACK_IMPORTED_MODULE_0__["Input"] },],
        "initDate": [{ type: _angular_core__WEBPACK_IMPORTED_MODULE_0__["Input"] },],
        "selectionDone": [{ type: _angular_core__WEBPACK_IMPORTED_MODULE_0__["Output"] },],
        "update": [{ type: _angular_core__WEBPACK_IMPORTED_MODULE_0__["Output"] },],
        "activeDateChange": [{ type: _angular_core__WEBPACK_IMPORTED_MODULE_0__["Output"] },],
        "activeDate": [{ type: _angular_core__WEBPACK_IMPORTED_MODULE_0__["Input"] },],
    };
    return DatePickerInnerComponent;
}());

//# sourceMappingURL=datepicker-inner.component.js.map

/***/ }),

/***/ "./node_modules/ngx-bootstrap/datepicker/datepicker.component.js":
/*!***********************************************************************!*\
  !*** ./node_modules/ngx-bootstrap/datepicker/datepicker.component.js ***!
  \***********************************************************************/
/*! exports provided: DATEPICKER_CONTROL_VALUE_ACCESSOR, DatePickerComponent */
/***/ (function(module, __webpack_exports__, __webpack_require__) {

"use strict";
__webpack_require__.r(__webpack_exports__);
/* harmony export (binding) */ __webpack_require__.d(__webpack_exports__, "DATEPICKER_CONTROL_VALUE_ACCESSOR", function() { return DATEPICKER_CONTROL_VALUE_ACCESSOR; });
/* harmony export (binding) */ __webpack_require__.d(__webpack_exports__, "DatePickerComponent", function() { return DatePickerComponent; });
/* harmony import */ var _angular_core__WEBPACK_IMPORTED_MODULE_0__ = __webpack_require__(/*! @angular/core */ "./node_modules/@angular/core/fesm5/core.js");
/* harmony import */ var _angular_forms__WEBPACK_IMPORTED_MODULE_1__ = __webpack_require__(/*! @angular/forms */ "./node_modules/@angular/forms/fesm5/forms.js");
/* harmony import */ var _datepicker_inner_component__WEBPACK_IMPORTED_MODULE_2__ = __webpack_require__(/*! ./datepicker-inner.component */ "./node_modules/ngx-bootstrap/datepicker/datepicker-inner.component.js");
/* harmony import */ var _datepicker_config__WEBPACK_IMPORTED_MODULE_3__ = __webpack_require__(/*! ./datepicker.config */ "./node_modules/ngx-bootstrap/datepicker/datepicker.config.js");




var DATEPICKER_CONTROL_VALUE_ACCESSOR = {
    provide: _angular_forms__WEBPACK_IMPORTED_MODULE_1__["NG_VALUE_ACCESSOR"],
    // tslint:disable-next-line
    useExisting: Object(_angular_core__WEBPACK_IMPORTED_MODULE_0__["forwardRef"])(function () { return DatePickerComponent; }),
    multi: true
};
/* tslint:disable:component-selector-name component-selector-type */
var DatePickerComponent = /** @class */ (function () {
    function DatePickerComponent(config) {
        /** sets datepicker mode, supports: `day`, `month`, `year` */
        this.datepickerMode = 'day';
        /** if false week numbers will be hidden */
        this.showWeeks = true;
        this.selectionDone = new _angular_core__WEBPACK_IMPORTED_MODULE_0__["EventEmitter"](undefined);
        /** callback to invoke when the activeDate is changed. */
        this.activeDateChange = new _angular_core__WEBPACK_IMPORTED_MODULE_0__["EventEmitter"](undefined);
        this.onChange = Function.prototype;
        this.onTouched = Function.prototype;
        this._now = new Date();
        this.config = config;
        this.configureOptions();
    }
    Object.defineProperty(DatePickerComponent.prototype, "activeDate", {
        get: /** currently active date */
        function () {
            return this._activeDate || this._now;
        },
        set: function (value) {
            this._activeDate = value;
        },
        enumerable: true,
        configurable: true
    });
    DatePickerComponent.prototype.configureOptions = function () {
        Object.assign(this, this.config);
    };
    DatePickerComponent.prototype.onUpdate = function (event) {
        this.activeDate = event;
        this.onChange(event);
    };
    DatePickerComponent.prototype.onSelectionDone = function (event) {
        this.selectionDone.emit(event);
    };
    DatePickerComponent.prototype.onActiveDateChange = function (event) {
        this.activeDateChange.emit(event);
    };
    // todo: support null value
    // todo: support null value
    DatePickerComponent.prototype.writeValue = 
    // todo: support null value
    function (value) {
        if (this._datePicker.compare(value, this._activeDate) === 0) {
            return;
        }
        if (value && value instanceof Date) {
            this.activeDate = value;
            this._datePicker.select(value, false);
            return;
        }
        this.activeDate = value ? new Date(value) : void 0;
    };
    DatePickerComponent.prototype.registerOnChange = function (fn) {
        this.onChange = fn;
    };
    DatePickerComponent.prototype.registerOnTouched = function (fn) {
        this.onTouched = fn;
    };
    DatePickerComponent.decorators = [
        { type: _angular_core__WEBPACK_IMPORTED_MODULE_0__["Component"], args: [{
                    selector: 'datepicker',
                    template: "\n    <datepicker-inner [activeDate]=\"activeDate\"\n                      (update)=\"onUpdate($event)\"\n                      [locale]=\"config.locale\"\n                      [datepickerMode]=\"datepickerMode\"\n                      [initDate]=\"initDate\"\n                      [minDate]=\"minDate\"\n                      [maxDate]=\"maxDate\"\n                      [minMode]=\"minMode\"\n                      [maxMode]=\"maxMode\"\n                      [showWeeks]=\"showWeeks\"\n                      [formatDay]=\"formatDay\"\n                      [formatMonth]=\"formatMonth\"\n                      [formatYear]=\"formatYear\"\n                      [formatDayHeader]=\"formatDayHeader\"\n                      [formatDayTitle]=\"formatDayTitle\"\n                      [formatMonthTitle]=\"formatMonthTitle\"\n                      [startingDay]=\"startingDay\"\n                      [yearRange]=\"yearRange\"\n                      [customClass]=\"customClass\"\n                      [dateDisabled]=\"dateDisabled\"\n                      [dayDisabled]=\"dayDisabled\"\n                      [onlyCurrentMonth]=\"onlyCurrentMonth\"\n                      [shortcutPropagation]=\"shortcutPropagation\"\n                      [monthColLimit]=\"monthColLimit\"\n                      [yearColLimit]=\"yearColLimit\"\n                      (selectionDone)=\"onSelectionDone($event)\"\n                      (activeDateChange)=\"onActiveDateChange($event)\">\n      <daypicker tabindex=\"0\"></daypicker>\n      <monthpicker tabindex=\"0\"></monthpicker>\n      <yearpicker tabindex=\"0\"></yearpicker>\n    </datepicker-inner>\n    ",
                    providers: [DATEPICKER_CONTROL_VALUE_ACCESSOR]
                },] },
    ];
    /** @nocollapse */
    DatePickerComponent.ctorParameters = function () { return [
        { type: _datepicker_config__WEBPACK_IMPORTED_MODULE_3__["DatepickerConfig"], },
    ]; };
    DatePickerComponent.propDecorators = {
        "datepickerMode": [{ type: _angular_core__WEBPACK_IMPORTED_MODULE_0__["Input"] },],
        "initDate": [{ type: _angular_core__WEBPACK_IMPORTED_MODULE_0__["Input"] },],
        "minDate": [{ type: _angular_core__WEBPACK_IMPORTED_MODULE_0__["Input"] },],
        "maxDate": [{ type: _angular_core__WEBPACK_IMPORTED_MODULE_0__["Input"] },],
        "minMode": [{ type: _angular_core__WEBPACK_IMPORTED_MODULE_0__["Input"] },],
        "maxMode": [{ type: _angular_core__WEBPACK_IMPORTED_MODULE_0__["Input"] },],
        "showWeeks": [{ type: _angular_core__WEBPACK_IMPORTED_MODULE_0__["Input"] },],
        "formatDay": [{ type: _angular_core__WEBPACK_IMPORTED_MODULE_0__["Input"] },],
        "formatMonth": [{ type: _angular_core__WEBPACK_IMPORTED_MODULE_0__["Input"] },],
        "formatYear": [{ type: _angular_core__WEBPACK_IMPORTED_MODULE_0__["Input"] },],
        "formatDayHeader": [{ type: _angular_core__WEBPACK_IMPORTED_MODULE_0__["Input"] },],
        "formatDayTitle": [{ type: _angular_core__WEBPACK_IMPORTED_MODULE_0__["Input"] },],
        "formatMonthTitle": [{ type: _angular_core__WEBPACK_IMPORTED_MODULE_0__["Input"] },],
        "startingDay": [{ type: _angular_core__WEBPACK_IMPORTED_MODULE_0__["Input"] },],
        "yearRange": [{ type: _angular_core__WEBPACK_IMPORTED_MODULE_0__["Input"] },],
        "onlyCurrentMonth": [{ type: _angular_core__WEBPACK_IMPORTED_MODULE_0__["Input"] },],
        "shortcutPropagation": [{ type: _angular_core__WEBPACK_IMPORTED_MODULE_0__["Input"] },],
        "monthColLimit": [{ type: _angular_core__WEBPACK_IMPORTED_MODULE_0__["Input"] },],
        "yearColLimit": [{ type: _angular_core__WEBPACK_IMPORTED_MODULE_0__["Input"] },],
        "customClass": [{ type: _angular_core__WEBPACK_IMPORTED_MODULE_0__["Input"] },],
        "dateDisabled": [{ type: _angular_core__WEBPACK_IMPORTED_MODULE_0__["Input"] },],
        "dayDisabled": [{ type: _angular_core__WEBPACK_IMPORTED_MODULE_0__["Input"] },],
        "activeDate": [{ type: _angular_core__WEBPACK_IMPORTED_MODULE_0__["Input"] },],
        "selectionDone": [{ type: _angular_core__WEBPACK_IMPORTED_MODULE_0__["Output"] },],
        "activeDateChange": [{ type: _angular_core__WEBPACK_IMPORTED_MODULE_0__["Output"] },],
        "_datePicker": [{ type: _angular_core__WEBPACK_IMPORTED_MODULE_0__["ViewChild"], args: [_datepicker_inner_component__WEBPACK_IMPORTED_MODULE_2__["DatePickerInnerComponent"],] },],
    };
    return DatePickerComponent;
}());

//# sourceMappingURL=datepicker.component.js.map

/***/ }),

/***/ "./node_modules/ngx-bootstrap/datepicker/datepicker.config.js":
/*!********************************************************************!*\
  !*** ./node_modules/ngx-bootstrap/datepicker/datepicker.config.js ***!
  \********************************************************************/
/*! exports provided: DatepickerConfig */
/***/ (function(module, __webpack_exports__, __webpack_require__) {

"use strict";
__webpack_require__.r(__webpack_exports__);
/* harmony export (binding) */ __webpack_require__.d(__webpack_exports__, "DatepickerConfig", function() { return DatepickerConfig; });
/* harmony import */ var _angular_core__WEBPACK_IMPORTED_MODULE_0__ = __webpack_require__(/*! @angular/core */ "./node_modules/@angular/core/fesm5/core.js");

var DatepickerConfig = /** @class */ (function () {
    function DatepickerConfig() {
        this.locale = 'en';
        this.datepickerMode = 'day';
        this.startingDay = 0;
        this.yearRange = 20;
        this.minMode = 'day';
        this.maxMode = 'year';
        this.showWeeks = true;
        this.formatDay = 'DD';
        this.formatMonth = 'MMMM';
        this.formatYear = 'YYYY';
        this.formatDayHeader = 'dd';
        this.formatDayTitle = 'MMMM YYYY';
        this.formatMonthTitle = 'YYYY';
        this.onlyCurrentMonth = false;
        this.monthColLimit = 3;
        this.yearColLimit = 5;
        this.shortcutPropagation = false;
    }
    DatepickerConfig.decorators = [
        { type: _angular_core__WEBPACK_IMPORTED_MODULE_0__["Injectable"] },
    ];
    return DatepickerConfig;
}());

//# sourceMappingURL=datepicker.config.js.map

/***/ }),

/***/ "./node_modules/ngx-bootstrap/datepicker/datepicker.module.js":
/*!********************************************************************!*\
  !*** ./node_modules/ngx-bootstrap/datepicker/datepicker.module.js ***!
  \********************************************************************/
/*! exports provided: DatepickerModule */
/***/ (function(module, __webpack_exports__, __webpack_require__) {

"use strict";
__webpack_require__.r(__webpack_exports__);
/* harmony export (binding) */ __webpack_require__.d(__webpack_exports__, "DatepickerModule", function() { return DatepickerModule; });
/* harmony import */ var _angular_common__WEBPACK_IMPORTED_MODULE_0__ = __webpack_require__(/*! @angular/common */ "./node_modules/@angular/common/fesm5/common.js");
/* harmony import */ var _angular_core__WEBPACK_IMPORTED_MODULE_1__ = __webpack_require__(/*! @angular/core */ "./node_modules/@angular/core/fesm5/core.js");
/* harmony import */ var _angular_forms__WEBPACK_IMPORTED_MODULE_2__ = __webpack_require__(/*! @angular/forms */ "./node_modules/@angular/forms/fesm5/forms.js");
/* harmony import */ var _datepicker_inner_component__WEBPACK_IMPORTED_MODULE_3__ = __webpack_require__(/*! ./datepicker-inner.component */ "./node_modules/ngx-bootstrap/datepicker/datepicker-inner.component.js");
/* harmony import */ var _datepicker_component__WEBPACK_IMPORTED_MODULE_4__ = __webpack_require__(/*! ./datepicker.component */ "./node_modules/ngx-bootstrap/datepicker/datepicker.component.js");
/* harmony import */ var _datepicker_config__WEBPACK_IMPORTED_MODULE_5__ = __webpack_require__(/*! ./datepicker.config */ "./node_modules/ngx-bootstrap/datepicker/datepicker.config.js");
/* harmony import */ var _daypicker_component__WEBPACK_IMPORTED_MODULE_6__ = __webpack_require__(/*! ./daypicker.component */ "./node_modules/ngx-bootstrap/datepicker/daypicker.component.js");
/* harmony import */ var _monthpicker_component__WEBPACK_IMPORTED_MODULE_7__ = __webpack_require__(/*! ./monthpicker.component */ "./node_modules/ngx-bootstrap/datepicker/monthpicker.component.js");
/* harmony import */ var _yearpicker_component__WEBPACK_IMPORTED_MODULE_8__ = __webpack_require__(/*! ./yearpicker.component */ "./node_modules/ngx-bootstrap/datepicker/yearpicker.component.js");









var DatepickerModule = /** @class */ (function () {
    function DatepickerModule() {
    }
    DatepickerModule.forRoot = function () {
        return { ngModule: DatepickerModule, providers: [_datepicker_config__WEBPACK_IMPORTED_MODULE_5__["DatepickerConfig"]] };
    };
    DatepickerModule.decorators = [
        { type: _angular_core__WEBPACK_IMPORTED_MODULE_1__["NgModule"], args: [{
                    imports: [_angular_common__WEBPACK_IMPORTED_MODULE_0__["CommonModule"], _angular_forms__WEBPACK_IMPORTED_MODULE_2__["FormsModule"]],
                    declarations: [
                        _datepicker_component__WEBPACK_IMPORTED_MODULE_4__["DatePickerComponent"],
                        _datepicker_inner_component__WEBPACK_IMPORTED_MODULE_3__["DatePickerInnerComponent"],
                        _daypicker_component__WEBPACK_IMPORTED_MODULE_6__["DayPickerComponent"],
                        _monthpicker_component__WEBPACK_IMPORTED_MODULE_7__["MonthPickerComponent"],
                        _yearpicker_component__WEBPACK_IMPORTED_MODULE_8__["YearPickerComponent"]
                    ],
                    exports: [
                        _datepicker_component__WEBPACK_IMPORTED_MODULE_4__["DatePickerComponent"],
                        _datepicker_inner_component__WEBPACK_IMPORTED_MODULE_3__["DatePickerInnerComponent"],
                        _daypicker_component__WEBPACK_IMPORTED_MODULE_6__["DayPickerComponent"],
                        _monthpicker_component__WEBPACK_IMPORTED_MODULE_7__["MonthPickerComponent"],
                        _yearpicker_component__WEBPACK_IMPORTED_MODULE_8__["YearPickerComponent"]
                    ],
                    entryComponents: [_datepicker_component__WEBPACK_IMPORTED_MODULE_4__["DatePickerComponent"]]
                },] },
    ];
    return DatepickerModule;
}());

//# sourceMappingURL=datepicker.module.js.map

/***/ }),

/***/ "./node_modules/ngx-bootstrap/datepicker/daypicker.component.js":
/*!**********************************************************************!*\
  !*** ./node_modules/ngx-bootstrap/datepicker/daypicker.component.js ***!
  \**********************************************************************/
/*! exports provided: DayPickerComponent */
/***/ (function(module, __webpack_exports__, __webpack_require__) {

"use strict";
__webpack_require__.r(__webpack_exports__);
/* harmony export (binding) */ __webpack_require__.d(__webpack_exports__, "DayPickerComponent", function() { return DayPickerComponent; });
/* harmony import */ var _angular_core__WEBPACK_IMPORTED_MODULE_0__ = __webpack_require__(/*! @angular/core */ "./node_modules/@angular/core/fesm5/core.js");
/* harmony import */ var _utils_theme_provider__WEBPACK_IMPORTED_MODULE_1__ = __webpack_require__(/*! ../utils/theme-provider */ "./node_modules/ngx-bootstrap/utils/theme-provider.js");
/* harmony import */ var _datepicker_inner_component__WEBPACK_IMPORTED_MODULE_2__ = __webpack_require__(/*! ./datepicker-inner.component */ "./node_modules/ngx-bootstrap/datepicker/datepicker-inner.component.js");



var DayPickerComponent = /** @class */ (function () {
    function DayPickerComponent(datePicker) {
        this.labels = [];
        this.rows = [];
        this.weekNumbers = [];
        this.datePicker = datePicker;
    }
    Object.defineProperty(DayPickerComponent.prototype, "isBs4", {
        get: function () {
            return !Object(_utils_theme_provider__WEBPACK_IMPORTED_MODULE_1__["isBs3"])();
        },
        enumerable: true,
        configurable: true
    });
    /*protected getDaysInMonth(year:number, month:number) {
     return ((month === 1) && (year % 4 === 0) &&
     ((year % 100 !== 0) || (year % 400 === 0))) ? 29 : DAYS_IN_MONTH[month];
     }*/
    /*protected getDaysInMonth(year:number, month:number) {
       return ((month === 1) && (year % 4 === 0) &&
       ((year % 100 !== 0) || (year % 400 === 0))) ? 29 : DAYS_IN_MONTH[month];
       }*/
    DayPickerComponent.prototype.ngOnInit = /*protected getDaysInMonth(year:number, month:number) {
       return ((month === 1) && (year % 4 === 0) &&
       ((year % 100 !== 0) || (year % 400 === 0))) ? 29 : DAYS_IN_MONTH[month];
       }*/
    function () {
        var self = this;
        this.datePicker.stepDay = { months: 1 };
        this.datePicker.setRefreshViewHandler(function () {
            var year = this.activeDate.getFullYear();
            var month = this.activeDate.getMonth();
            var firstDayOfMonth = new Date(year, month, 1);
            var difference = this.startingDay - firstDayOfMonth.getDay();
            var numDisplayedFromPreviousMonth = difference > 0 ? 7 - difference : -difference;
            var firstDate = new Date(firstDayOfMonth.getTime());
            if (numDisplayedFromPreviousMonth > 0) {
                firstDate.setDate(-numDisplayedFromPreviousMonth + 1);
            }
            // 42 is the number of days on a six-week calendar
            var _days = self.getDates(firstDate, 42);
            var days = [];
            for (var i = 0; i < 42; i++) {
                var _dateObject = this.createDateObject(_days[i], this.formatDay);
                _dateObject.secondary = _days[i].getMonth() !== month;
                _dateObject.uid = this.uniqueId + '-' + i;
                days[i] = _dateObject;
            }
            self.labels = [];
            for (var j = 0; j < 7; j++) {
                self.labels[j] = {};
                self.labels[j].abbr = this.dateFilter(days[j].date, this.formatDayHeader);
                self.labels[j].full = this.dateFilter(days[j].date, 'EEEE');
            }
            self.title = this.dateFilter(this.activeDate, this.formatDayTitle);
            self.rows = this.split(days, 7);
            if (this.showWeeks) {
                self.weekNumbers = [];
                var thursdayIndex = (4 + 7 - this.startingDay) % 7;
                var numWeeks = self.rows.length;
                for (var curWeek = 0; curWeek < numWeeks; curWeek++) {
                    self.weekNumbers.push(self.getISO8601WeekNumber(self.rows[curWeek][thursdayIndex].date));
                }
            }
        }, 'day');
        this.datePicker.setCompareHandler(function (date1, date2) {
            var d1 = new Date(date1.getFullYear(), date1.getMonth(), date1.getDate());
            var d2 = new Date(date2.getFullYear(), date2.getMonth(), date2.getDate());
            return d1.getTime() - d2.getTime();
        }, 'day');
        this.datePicker.refreshView();
    };
    DayPickerComponent.prototype.getDates = function (startDate, n) {
        var dates = new Array(n);
        var current = new Date(startDate.getTime());
        var i = 0;
        var date;
        while (i < n) {
            date = new Date(current.getTime());
            date = this.datePicker.fixTimeZone(date);
            dates[i++] = date;
            current = new Date(date.getFullYear(), date.getMonth(), date.getDate() + 1);
        }
        return dates;
    };
    DayPickerComponent.prototype.getISO8601WeekNumber = function (date) {
        var checkDate = new Date(date.getTime());
        // Thursday
        checkDate.setDate(checkDate.getDate() + 4 - (checkDate.getDay() || 7));
        var time = checkDate.getTime();
        // Compare with Jan 1
        checkDate.setMonth(0);
        checkDate.setDate(1);
        return (Math.floor(Math.round((time - checkDate.getTime()) / 86400000) / 7) + 1);
    };
    DayPickerComponent.decorators = [
        { type: _angular_core__WEBPACK_IMPORTED_MODULE_0__["Component"], args: [{
                    selector: 'daypicker',
                    template: "\n<table *ngIf=\"datePicker.datepickerMode === 'day'\" role=\"grid\" [attr.aria-labelledby]=\"datePicker.uniqueId + '-title'\" aria-activedescendant=\"activeDateId\">\n  <thead>\n    <tr>\n      <th>\n        <button *ngIf=\"!isBs4\"\n                type=\"button\"\n                class=\"btn btn-default btn-secondary btn-sm pull-left float-left\"\n                (click)=\"datePicker.move(-1)\"\n                tabindex=\"-1\">\u2039</button>\n        <button *ngIf=\"isBs4\"\n                type=\"button\"\n                class=\"btn btn-default btn-secondary btn-sm pull-left float-left\"\n                (click)=\"datePicker.move(-1)\"\n                tabindex=\"-1\">&lt;</button>\n      </th>\n      <th [attr.colspan]=\"5 + (datePicker.showWeeks ? 1 : 0)\">\n        <button [id]=\"datePicker.uniqueId + '-title'\"\n                type=\"button\" class=\"btn btn-default btn-secondary btn-sm\"\n                (click)=\"datePicker.toggleMode(0)\"\n                [disabled]=\"datePicker.datepickerMode === datePicker.maxMode\"\n                [ngClass]=\"{disabled: datePicker.datepickerMode === datePicker.maxMode}\" tabindex=\"-1\" style=\"width:100%;\">\n          <strong>{{ title }}</strong>\n        </button>\n      </th>\n      <th>\n        <button *ngIf=\"!isBs4\"\n                type=\"button\"\n                class=\"btn btn-default btn-secondary btn-sm pull-right float-right\"\n                (click)=\"datePicker.move(1)\"\n                tabindex=\"-1\">\u203A</button>\n        <button *ngIf=\"isBs4\"\n                type=\"button\"\n                class=\"btn btn-default btn-secondary btn-sm pull-right float-right\"\n                (click)=\"datePicker.move(1)\"\n                tabindex=\"-1\">&gt;\n        </button>\n      </th>\n    </tr>\n    <tr>\n      <th *ngIf=\"datePicker.showWeeks\"></th>\n      <th *ngFor=\"let labelz of labels\" class=\"text-center\">\n        <small aria-label=\"labelz.full\"><b>{{ labelz.abbr }}</b></small>\n      </th>\n    </tr>\n  </thead>\n  <tbody>\n    <ng-template ngFor [ngForOf]=\"rows\" let-rowz=\"$implicit\" let-index=\"index\">\n      <tr *ngIf=\"!(datePicker.onlyCurrentMonth && rowz[0].secondary && rowz[6].secondary)\">\n        <td *ngIf=\"datePicker.showWeeks\" class=\"h6\" class=\"text-center\">\n          <em>{{ weekNumbers[index] }}</em>\n        </td>\n        <td *ngFor=\"let dtz of rowz\" class=\"text-center\" role=\"gridcell\" [id]=\"dtz.uid\">\n          <button type=\"button\" style=\"min-width:100%;\" class=\"btn btn-sm {{dtz.customClass}}\"\n                  *ngIf=\"!(datePicker.onlyCurrentMonth && dtz.secondary)\"\n                  [ngClass]=\"{'btn-secondary': isBs4 && !dtz.selected && !datePicker.isActive(dtz), 'btn-info': dtz.selected, disabled: dtz.disabled, active: !isBs4 && datePicker.isActive(dtz), 'btn-default': !isBs4}\"\n                  [disabled]=\"dtz.disabled\"\n                  (click)=\"datePicker.select(dtz.date)\" tabindex=\"-1\">\n            <span [ngClass]=\"{'text-muted': dtz.secondary || dtz.current, 'text-info': !isBs4 && dtz.current}\">{{ dtz.label }}</span>\n          </button>\n        </td>\n      </tr>\n    </ng-template>\n  </tbody>\n</table>\n  ",
                    styles: [
                        "\n    :host .btn-secondary {\n      color: #292b2c;\n      background-color: #fff;\n      border-color: #ccc;\n    }\n    :host .btn-info .text-muted {\n      color: #292b2c !important;\n    }\n  "
                    ]
                },] },
    ];
    // todo: key events implementation
    /** @nocollapse */
    DayPickerComponent.ctorParameters = function () { return [
        { type: _datepicker_inner_component__WEBPACK_IMPORTED_MODULE_2__["DatePickerInnerComponent"], },
    ]; };
    return DayPickerComponent;
}());

//# sourceMappingURL=daypicker.component.js.map

/***/ }),

/***/ "./node_modules/ngx-bootstrap/datepicker/engine/calc-days-calendar.js":
/*!****************************************************************************!*\
  !*** ./node_modules/ngx-bootstrap/datepicker/engine/calc-days-calendar.js ***!
  \****************************************************************************/
/*! exports provided: calcDaysCalendar */
/***/ (function(module, __webpack_exports__, __webpack_require__) {

"use strict";
__webpack_require__.r(__webpack_exports__);
/* harmony export (binding) */ __webpack_require__.d(__webpack_exports__, "calcDaysCalendar", function() { return calcDaysCalendar; });
/* harmony import */ var _chronos_utils_date_getters__WEBPACK_IMPORTED_MODULE_0__ = __webpack_require__(/*! ../../chronos/utils/date-getters */ "./node_modules/ngx-bootstrap/chronos/utils/date-getters.js");
/* harmony import */ var _utils_bs_calendar_utils__WEBPACK_IMPORTED_MODULE_1__ = __webpack_require__(/*! ../utils/bs-calendar-utils */ "./node_modules/ngx-bootstrap/datepicker/utils/bs-calendar-utils.js");
/* harmony import */ var _utils_matrix_utils__WEBPACK_IMPORTED_MODULE_2__ = __webpack_require__(/*! ../utils/matrix-utils */ "./node_modules/ngx-bootstrap/datepicker/utils/matrix-utils.js");



function calcDaysCalendar(startingDate, options) {
    var firstDay = Object(_chronos_utils_date_getters__WEBPACK_IMPORTED_MODULE_0__["getFirstDayOfMonth"])(startingDate);
    var initialDate = Object(_utils_bs_calendar_utils__WEBPACK_IMPORTED_MODULE_1__["getStartingDayOfCalendar"])(firstDay, options);
    var matrixOptions = {
        width: options.width,
        height: options.height,
        initialDate: initialDate,
        shift: { day: 1 }
    };
    var daysMatrix = Object(_utils_matrix_utils__WEBPACK_IMPORTED_MODULE_2__["createMatrix"])(matrixOptions, function (date) { return date; });
    return {
        daysMatrix: daysMatrix,
        month: firstDay
    };
}
//# sourceMappingURL=calc-days-calendar.js.map

/***/ }),

/***/ "./node_modules/ngx-bootstrap/datepicker/engine/flag-days-calendar.js":
/*!****************************************************************************!*\
  !*** ./node_modules/ngx-bootstrap/datepicker/engine/flag-days-calendar.js ***!
  \****************************************************************************/
/*! exports provided: flagDaysCalendar */
/***/ (function(module, __webpack_exports__, __webpack_require__) {

"use strict";
__webpack_require__.r(__webpack_exports__);
/* harmony export (binding) */ __webpack_require__.d(__webpack_exports__, "flagDaysCalendar", function() { return flagDaysCalendar; });
/* harmony import */ var _chronos_utils_date_getters__WEBPACK_IMPORTED_MODULE_0__ = __webpack_require__(/*! ../../chronos/utils/date-getters */ "./node_modules/ngx-bootstrap/chronos/utils/date-getters.js");
/* harmony import */ var _chronos_utils_date_compare__WEBPACK_IMPORTED_MODULE_1__ = __webpack_require__(/*! ../../chronos/utils/date-compare */ "./node_modules/ngx-bootstrap/chronos/utils/date-compare.js");
/* harmony import */ var _utils_bs_calendar_utils__WEBPACK_IMPORTED_MODULE_2__ = __webpack_require__(/*! ../utils/bs-calendar-utils */ "./node_modules/ngx-bootstrap/datepicker/utils/bs-calendar-utils.js");
/* harmony import */ var _chronos_utils_date_setters__WEBPACK_IMPORTED_MODULE_3__ = __webpack_require__(/*! ../../chronos/utils/date-setters */ "./node_modules/ngx-bootstrap/chronos/utils/date-setters.js");




function flagDaysCalendar(formattedMonth, options) {
    formattedMonth.weeks.forEach(function (week, weekIndex) {
        week.days.forEach(function (day, dayIndex) {
            // datepicker
            var isOtherMonth = !Object(_chronos_utils_date_getters__WEBPACK_IMPORTED_MODULE_0__["isSameMonth"])(day.date, formattedMonth.month);
            var isHovered = !isOtherMonth && Object(_chronos_utils_date_getters__WEBPACK_IMPORTED_MODULE_0__["isSameDay"])(day.date, options.hoveredDate);
            // date range picker
            var isSelectionStart = !isOtherMonth &&
                options.selectedRange &&
                Object(_chronos_utils_date_getters__WEBPACK_IMPORTED_MODULE_0__["isSameDay"])(day.date, options.selectedRange[0]);
            var isSelectionEnd = !isOtherMonth &&
                options.selectedRange &&
                Object(_chronos_utils_date_getters__WEBPACK_IMPORTED_MODULE_0__["isSameDay"])(day.date, options.selectedRange[1]);
            var isSelected = (!isOtherMonth && Object(_chronos_utils_date_getters__WEBPACK_IMPORTED_MODULE_0__["isSameDay"])(day.date, options.selectedDate)) ||
                isSelectionStart ||
                isSelectionEnd;
            var isInRange = !isOtherMonth &&
                options.selectedRange &&
                isDateInRange(day.date, options.selectedRange, options.hoveredDate);
            var isDisabled = options.isDisabled ||
                Object(_chronos_utils_date_compare__WEBPACK_IMPORTED_MODULE_1__["isBefore"])(day.date, options.minDate, 'day') ||
                Object(_chronos_utils_date_compare__WEBPACK_IMPORTED_MODULE_1__["isAfter"])(day.date, options.maxDate, 'day');
            // decide update or not
            var newDay = Object.assign({}, day, {
                isOtherMonth: isOtherMonth,
                isHovered: isHovered,
                isSelected: isSelected,
                isSelectionStart: isSelectionStart,
                isSelectionEnd: isSelectionEnd,
                isInRange: isInRange,
                isDisabled: isDisabled
            });
            if (day.isOtherMonth !== newDay.isOtherMonth ||
                day.isHovered !== newDay.isHovered ||
                day.isSelected !== newDay.isSelected ||
                day.isSelectionStart !== newDay.isSelectionStart ||
                day.isSelectionEnd !== newDay.isSelectionEnd ||
                day.isDisabled !== newDay.isDisabled ||
                day.isInRange !== newDay.isInRange) {
                week.days[dayIndex] = newDay;
            }
        });
    });
    // todo: add check for linked calendars
    formattedMonth.hideLeftArrow =
        options.isDisabled ||
            (options.monthIndex > 0 && options.monthIndex !== options.displayMonths);
    formattedMonth.hideRightArrow =
        options.isDisabled ||
            (options.monthIndex < options.displayMonths &&
                options.monthIndex + 1 !== options.displayMonths);
    formattedMonth.disableLeftArrow = Object(_utils_bs_calendar_utils__WEBPACK_IMPORTED_MODULE_2__["isMonthDisabled"])(Object(_chronos_utils_date_setters__WEBPACK_IMPORTED_MODULE_3__["shiftDate"])(formattedMonth.month, { month: -1 }), options.minDate, options.maxDate);
    formattedMonth.disableRightArrow = Object(_utils_bs_calendar_utils__WEBPACK_IMPORTED_MODULE_2__["isMonthDisabled"])(Object(_chronos_utils_date_setters__WEBPACK_IMPORTED_MODULE_3__["shiftDate"])(formattedMonth.month, { month: 1 }), options.minDate, options.maxDate);
    return formattedMonth;
}
function isDateInRange(date, selectedRange, hoveredDate) {
    if (!date || !selectedRange[0]) {
        return false;
    }
    if (selectedRange[1]) {
        return date > selectedRange[0] && date <= selectedRange[1];
    }
    if (hoveredDate) {
        return date > selectedRange[0] && date <= hoveredDate;
    }
    return false;
}
//# sourceMappingURL=flag-days-calendar.js.map

/***/ }),

/***/ "./node_modules/ngx-bootstrap/datepicker/engine/flag-months-calendar.js":
/*!******************************************************************************!*\
  !*** ./node_modules/ngx-bootstrap/datepicker/engine/flag-months-calendar.js ***!
  \******************************************************************************/
/*! exports provided: flagMonthsCalendar */
/***/ (function(module, __webpack_exports__, __webpack_require__) {

"use strict";
__webpack_require__.r(__webpack_exports__);
/* harmony export (binding) */ __webpack_require__.d(__webpack_exports__, "flagMonthsCalendar", function() { return flagMonthsCalendar; });
/* harmony import */ var _chronos_utils_date_getters__WEBPACK_IMPORTED_MODULE_0__ = __webpack_require__(/*! ../../chronos/utils/date-getters */ "./node_modules/ngx-bootstrap/chronos/utils/date-getters.js");
/* harmony import */ var _utils_bs_calendar_utils__WEBPACK_IMPORTED_MODULE_1__ = __webpack_require__(/*! ../utils/bs-calendar-utils */ "./node_modules/ngx-bootstrap/datepicker/utils/bs-calendar-utils.js");
/* harmony import */ var _chronos_utils_date_setters__WEBPACK_IMPORTED_MODULE_2__ = __webpack_require__(/*! ../../chronos/utils/date-setters */ "./node_modules/ngx-bootstrap/chronos/utils/date-setters.js");



function flagMonthsCalendar(monthCalendar, options) {
    monthCalendar.months.forEach(function (months, rowIndex) {
        months.forEach(function (month, monthIndex) {
            var isHovered = Object(_chronos_utils_date_getters__WEBPACK_IMPORTED_MODULE_0__["isSameMonth"])(month.date, options.hoveredMonth);
            var isDisabled = options.isDisabled ||
                Object(_utils_bs_calendar_utils__WEBPACK_IMPORTED_MODULE_1__["isMonthDisabled"])(month.date, options.minDate, options.maxDate);
            var newMonth = Object.assign(/*{},*/ month, {
                isHovered: isHovered,
                isDisabled: isDisabled
            });
            if (month.isHovered !== newMonth.isHovered ||
                month.isDisabled !== newMonth.isDisabled) {
                monthCalendar.months[rowIndex][monthIndex] = newMonth;
            }
        });
    });
    // todo: add check for linked calendars
    monthCalendar.hideLeftArrow =
        options.monthIndex > 0 && options.monthIndex !== options.displayMonths;
    monthCalendar.hideRightArrow =
        options.monthIndex < options.displayMonths &&
            options.monthIndex + 1 !== options.displayMonths;
    monthCalendar.disableLeftArrow = Object(_utils_bs_calendar_utils__WEBPACK_IMPORTED_MODULE_1__["isYearDisabled"])(Object(_chronos_utils_date_setters__WEBPACK_IMPORTED_MODULE_2__["shiftDate"])(monthCalendar.months[0][0].date, { year: -1 }), options.minDate, options.maxDate);
    monthCalendar.disableRightArrow = Object(_utils_bs_calendar_utils__WEBPACK_IMPORTED_MODULE_1__["isYearDisabled"])(Object(_chronos_utils_date_setters__WEBPACK_IMPORTED_MODULE_2__["shiftDate"])(monthCalendar.months[0][0].date, { year: 1 }), options.minDate, options.maxDate);
    return monthCalendar;
}
//# sourceMappingURL=flag-months-calendar.js.map

/***/ }),

/***/ "./node_modules/ngx-bootstrap/datepicker/engine/flag-years-calendar.js":
/*!*****************************************************************************!*\
  !*** ./node_modules/ngx-bootstrap/datepicker/engine/flag-years-calendar.js ***!
  \*****************************************************************************/
/*! exports provided: flagYearsCalendar */
/***/ (function(module, __webpack_exports__, __webpack_require__) {

"use strict";
__webpack_require__.r(__webpack_exports__);
/* harmony export (binding) */ __webpack_require__.d(__webpack_exports__, "flagYearsCalendar", function() { return flagYearsCalendar; });
/* harmony import */ var _chronos_utils_date_getters__WEBPACK_IMPORTED_MODULE_0__ = __webpack_require__(/*! ../../chronos/utils/date-getters */ "./node_modules/ngx-bootstrap/chronos/utils/date-getters.js");
/* harmony import */ var _utils_bs_calendar_utils__WEBPACK_IMPORTED_MODULE_1__ = __webpack_require__(/*! ../utils/bs-calendar-utils */ "./node_modules/ngx-bootstrap/datepicker/utils/bs-calendar-utils.js");
/* harmony import */ var _chronos_utils_date_setters__WEBPACK_IMPORTED_MODULE_2__ = __webpack_require__(/*! ../../chronos/utils/date-setters */ "./node_modules/ngx-bootstrap/chronos/utils/date-setters.js");



function flagYearsCalendar(yearsCalendar, options) {
    yearsCalendar.years.forEach(function (years, rowIndex) {
        years.forEach(function (year, yearIndex) {
            var isHovered = Object(_chronos_utils_date_getters__WEBPACK_IMPORTED_MODULE_0__["isSameYear"])(year.date, options.hoveredYear);
            var isDisabled = options.isDisabled ||
                Object(_utils_bs_calendar_utils__WEBPACK_IMPORTED_MODULE_1__["isYearDisabled"])(year.date, options.minDate, options.maxDate);
            var newMonth = Object.assign(/*{},*/ year, { isHovered: isHovered, isDisabled: isDisabled });
            if (year.isHovered !== newMonth.isHovered ||
                year.isDisabled !== newMonth.isDisabled) {
                yearsCalendar.years[rowIndex][yearIndex] = newMonth;
            }
        });
    });
    // todo: add check for linked calendars
    yearsCalendar.hideLeftArrow =
        options.yearIndex > 0 && options.yearIndex !== options.displayMonths;
    yearsCalendar.hideRightArrow =
        options.yearIndex < options.displayMonths &&
            options.yearIndex + 1 !== options.displayMonths;
    yearsCalendar.disableLeftArrow = Object(_utils_bs_calendar_utils__WEBPACK_IMPORTED_MODULE_1__["isYearDisabled"])(Object(_chronos_utils_date_setters__WEBPACK_IMPORTED_MODULE_2__["shiftDate"])(yearsCalendar.years[0][0].date, { year: -1 }), options.minDate, options.maxDate);
    var i = yearsCalendar.years.length - 1;
    var j = yearsCalendar.years[i].length - 1;
    yearsCalendar.disableRightArrow = Object(_utils_bs_calendar_utils__WEBPACK_IMPORTED_MODULE_1__["isYearDisabled"])(Object(_chronos_utils_date_setters__WEBPACK_IMPORTED_MODULE_2__["shiftDate"])(yearsCalendar.years[i][j].date, { year: 1 }), options.minDate, options.maxDate);
    return yearsCalendar;
}
//# sourceMappingURL=flag-years-calendar.js.map

/***/ }),

/***/ "./node_modules/ngx-bootstrap/datepicker/engine/format-days-calendar.js":
/*!******************************************************************************!*\
  !*** ./node_modules/ngx-bootstrap/datepicker/engine/format-days-calendar.js ***!
  \******************************************************************************/
/*! exports provided: formatDaysCalendar, getWeekNumbers, getShiftedWeekdays */
/***/ (function(module, __webpack_exports__, __webpack_require__) {

"use strict";
__webpack_require__.r(__webpack_exports__);
/* harmony export (binding) */ __webpack_require__.d(__webpack_exports__, "formatDaysCalendar", function() { return formatDaysCalendar; });
/* harmony export (binding) */ __webpack_require__.d(__webpack_exports__, "getWeekNumbers", function() { return getWeekNumbers; });
/* harmony export (binding) */ __webpack_require__.d(__webpack_exports__, "getShiftedWeekdays", function() { return getShiftedWeekdays; });
/* harmony import */ var _chronos_format__WEBPACK_IMPORTED_MODULE_0__ = __webpack_require__(/*! ../../chronos/format */ "./node_modules/ngx-bootstrap/chronos/format.js");
/* harmony import */ var _chronos_locale_locales__WEBPACK_IMPORTED_MODULE_1__ = __webpack_require__(/*! ../../chronos/locale/locales */ "./node_modules/ngx-bootstrap/chronos/locale/locales.js");


function formatDaysCalendar(daysCalendar, formatOptions, monthIndex) {
    return {
        month: daysCalendar.month,
        monthTitle: Object(_chronos_format__WEBPACK_IMPORTED_MODULE_0__["formatDate"])(daysCalendar.month, formatOptions.monthTitle, formatOptions.locale),
        yearTitle: Object(_chronos_format__WEBPACK_IMPORTED_MODULE_0__["formatDate"])(daysCalendar.month, formatOptions.yearTitle, formatOptions.locale),
        weekNumbers: getWeekNumbers(daysCalendar.daysMatrix, formatOptions.weekNumbers, formatOptions.locale),
        weekdays: getShiftedWeekdays(formatOptions.locale),
        weeks: daysCalendar.daysMatrix.map(function (week, weekIndex) {
            return ({
                days: week.map(function (date, dayIndex) {
                    return ({
                        date: date,
                        label: Object(_chronos_format__WEBPACK_IMPORTED_MODULE_0__["formatDate"])(date, formatOptions.dayLabel, formatOptions.locale),
                        monthIndex: monthIndex,
                        weekIndex: weekIndex,
                        dayIndex: dayIndex
                    });
                })
            });
        })
    };
}
function getWeekNumbers(daysMatrix, format, locale) {
    return daysMatrix.map(function (days) { return (days[0] ? Object(_chronos_format__WEBPACK_IMPORTED_MODULE_0__["formatDate"])(days[0], format, locale) : ''); });
}
function getShiftedWeekdays(locale) {
    var _locale = Object(_chronos_locale_locales__WEBPACK_IMPORTED_MODULE_1__["getLocale"])(locale);
    var weekdays = _locale.weekdaysShort();
    var firstDayOfWeek = _locale.firstDayOfWeek();
    return weekdays.slice(firstDayOfWeek).concat(weekdays.slice(0, firstDayOfWeek));
}
//# sourceMappingURL=format-days-calendar.js.map

/***/ }),

/***/ "./node_modules/ngx-bootstrap/datepicker/engine/format-months-calendar.js":
/*!********************************************************************************!*\
  !*** ./node_modules/ngx-bootstrap/datepicker/engine/format-months-calendar.js ***!
  \********************************************************************************/
/*! exports provided: formatMonthsCalendar */
/***/ (function(module, __webpack_exports__, __webpack_require__) {

"use strict";
__webpack_require__.r(__webpack_exports__);
/* harmony export (binding) */ __webpack_require__.d(__webpack_exports__, "formatMonthsCalendar", function() { return formatMonthsCalendar; });
/* harmony import */ var _chronos_utils_start_end_of__WEBPACK_IMPORTED_MODULE_0__ = __webpack_require__(/*! ../../chronos/utils/start-end-of */ "./node_modules/ngx-bootstrap/chronos/utils/start-end-of.js");
/* harmony import */ var _chronos_format__WEBPACK_IMPORTED_MODULE_1__ = __webpack_require__(/*! ../../chronos/format */ "./node_modules/ngx-bootstrap/chronos/format.js");
/* harmony import */ var _utils_matrix_utils__WEBPACK_IMPORTED_MODULE_2__ = __webpack_require__(/*! ../utils/matrix-utils */ "./node_modules/ngx-bootstrap/datepicker/utils/matrix-utils.js");



var height = 4;
var width = 3;
var shift = { month: 1 };
function formatMonthsCalendar(viewDate, formatOptions) {
    var initialDate = Object(_chronos_utils_start_end_of__WEBPACK_IMPORTED_MODULE_0__["startOf"])(viewDate, 'year');
    var matrixOptions = { width: width, height: height, initialDate: initialDate, shift: shift };
    var monthMatrix = Object(_utils_matrix_utils__WEBPACK_IMPORTED_MODULE_2__["createMatrix"])(matrixOptions, function (date) {
        return ({
            date: date,
            label: Object(_chronos_format__WEBPACK_IMPORTED_MODULE_1__["formatDate"])(date, formatOptions.monthLabel, formatOptions.locale)
        });
    });
    return {
        months: monthMatrix,
        monthTitle: '',
        yearTitle: Object(_chronos_format__WEBPACK_IMPORTED_MODULE_1__["formatDate"])(viewDate, formatOptions.yearTitle, formatOptions.locale)
    };
}
//# sourceMappingURL=format-months-calendar.js.map

/***/ }),

/***/ "./node_modules/ngx-bootstrap/datepicker/engine/format-years-calendar.js":
/*!*******************************************************************************!*\
  !*** ./node_modules/ngx-bootstrap/datepicker/engine/format-years-calendar.js ***!
  \*******************************************************************************/
/*! exports provided: yearsPerCalendar, formatYearsCalendar */
/***/ (function(module, __webpack_exports__, __webpack_require__) {

"use strict";
__webpack_require__.r(__webpack_exports__);
/* harmony export (binding) */ __webpack_require__.d(__webpack_exports__, "yearsPerCalendar", function() { return yearsPerCalendar; });
/* harmony export (binding) */ __webpack_require__.d(__webpack_exports__, "formatYearsCalendar", function() { return formatYearsCalendar; });
/* harmony import */ var _chronos_utils_date_setters__WEBPACK_IMPORTED_MODULE_0__ = __webpack_require__(/*! ../../chronos/utils/date-setters */ "./node_modules/ngx-bootstrap/chronos/utils/date-setters.js");
/* harmony import */ var _chronos_format__WEBPACK_IMPORTED_MODULE_1__ = __webpack_require__(/*! ../../chronos/format */ "./node_modules/ngx-bootstrap/chronos/format.js");
/* harmony import */ var _utils_matrix_utils__WEBPACK_IMPORTED_MODULE_2__ = __webpack_require__(/*! ../utils/matrix-utils */ "./node_modules/ngx-bootstrap/datepicker/utils/matrix-utils.js");



var height = 4;
var width = 4;
var yearsPerCalendar = height * width;
var initialShift = (Math.floor(yearsPerCalendar / 2) - 1) * -1;
var shift = { year: 1 };
function formatYearsCalendar(viewDate, formatOptions) {
    var initialDate = Object(_chronos_utils_date_setters__WEBPACK_IMPORTED_MODULE_0__["shiftDate"])(viewDate, { year: initialShift });
    var matrixOptions = { width: width, height: height, initialDate: initialDate, shift: shift };
    var yearsMatrix = Object(_utils_matrix_utils__WEBPACK_IMPORTED_MODULE_2__["createMatrix"])(matrixOptions, function (date) {
        return ({
            date: date,
            label: Object(_chronos_format__WEBPACK_IMPORTED_MODULE_1__["formatDate"])(date, formatOptions.yearLabel, formatOptions.locale)
        });
    });
    var yearTitle = formatYearRangeTitle(yearsMatrix, formatOptions);
    return {
        years: yearsMatrix,
        monthTitle: '',
        yearTitle: yearTitle
    };
}
function formatYearRangeTitle(yearsMatrix, formatOptions) {
    var from = Object(_chronos_format__WEBPACK_IMPORTED_MODULE_1__["formatDate"])(yearsMatrix[0][0].date, formatOptions.yearTitle, formatOptions.locale);
    var to = Object(_chronos_format__WEBPACK_IMPORTED_MODULE_1__["formatDate"])(yearsMatrix[height - 1][width - 1].date, formatOptions.yearTitle, formatOptions.locale);
    return from + " - " + to;
}
//# sourceMappingURL=format-years-calendar.js.map

/***/ }),

/***/ "./node_modules/ngx-bootstrap/datepicker/engine/view-mode.js":
/*!*******************************************************************!*\
  !*** ./node_modules/ngx-bootstrap/datepicker/engine/view-mode.js ***!
  \*******************************************************************/
/*! exports provided: canSwitchMode */
/***/ (function(module, __webpack_exports__, __webpack_require__) {

"use strict";
__webpack_require__.r(__webpack_exports__);
/* harmony export (binding) */ __webpack_require__.d(__webpack_exports__, "canSwitchMode", function() { return canSwitchMode; });
function canSwitchMode(mode) {
    return true;
}
//# sourceMappingURL=view-mode.js.map

/***/ }),

/***/ "./node_modules/ngx-bootstrap/datepicker/index.js":
/*!********************************************************!*\
  !*** ./node_modules/ngx-bootstrap/datepicker/index.js ***!
  \********************************************************/
/*! exports provided: DatePickerComponent, DatepickerModule, DayPickerComponent, MonthPickerComponent, YearPickerComponent, DateFormatter, DatepickerConfig, BsDatepickerModule, BsDatepickerDirective, BsDaterangepickerDirective, BsDatepickerConfig, BsDaterangepickerConfig, BsLocaleService */
/***/ (function(module, __webpack_exports__, __webpack_require__) {

"use strict";
__webpack_require__.r(__webpack_exports__);
/* harmony import */ var _datepicker_component__WEBPACK_IMPORTED_MODULE_0__ = __webpack_require__(/*! ./datepicker.component */ "./node_modules/ngx-bootstrap/datepicker/datepicker.component.js");
/* harmony reexport (safe) */ __webpack_require__.d(__webpack_exports__, "DatePickerComponent", function() { return _datepicker_component__WEBPACK_IMPORTED_MODULE_0__["DatePickerComponent"]; });

/* harmony import */ var _datepicker_module__WEBPACK_IMPORTED_MODULE_1__ = __webpack_require__(/*! ./datepicker.module */ "./node_modules/ngx-bootstrap/datepicker/datepicker.module.js");
/* harmony reexport (safe) */ __webpack_require__.d(__webpack_exports__, "DatepickerModule", function() { return _datepicker_module__WEBPACK_IMPORTED_MODULE_1__["DatepickerModule"]; });

/* harmony import */ var _daypicker_component__WEBPACK_IMPORTED_MODULE_2__ = __webpack_require__(/*! ./daypicker.component */ "./node_modules/ngx-bootstrap/datepicker/daypicker.component.js");
/* harmony reexport (safe) */ __webpack_require__.d(__webpack_exports__, "DayPickerComponent", function() { return _daypicker_component__WEBPACK_IMPORTED_MODULE_2__["DayPickerComponent"]; });

/* harmony import */ var _monthpicker_component__WEBPACK_IMPORTED_MODULE_3__ = __webpack_require__(/*! ./monthpicker.component */ "./node_modules/ngx-bootstrap/datepicker/monthpicker.component.js");
/* harmony reexport (safe) */ __webpack_require__.d(__webpack_exports__, "MonthPickerComponent", function() { return _monthpicker_component__WEBPACK_IMPORTED_MODULE_3__["MonthPickerComponent"]; });

/* harmony import */ var _yearpicker_component__WEBPACK_IMPORTED_MODULE_4__ = __webpack_require__(/*! ./yearpicker.component */ "./node_modules/ngx-bootstrap/datepicker/yearpicker.component.js");
/* harmony reexport (safe) */ __webpack_require__.d(__webpack_exports__, "YearPickerComponent", function() { return _yearpicker_component__WEBPACK_IMPORTED_MODULE_4__["YearPickerComponent"]; });

/* harmony import */ var _date_formatter__WEBPACK_IMPORTED_MODULE_5__ = __webpack_require__(/*! ./date-formatter */ "./node_modules/ngx-bootstrap/datepicker/date-formatter.js");
/* harmony reexport (safe) */ __webpack_require__.d(__webpack_exports__, "DateFormatter", function() { return _date_formatter__WEBPACK_IMPORTED_MODULE_5__["DateFormatter"]; });

/* harmony import */ var _datepicker_config__WEBPACK_IMPORTED_MODULE_6__ = __webpack_require__(/*! ./datepicker.config */ "./node_modules/ngx-bootstrap/datepicker/datepicker.config.js");
/* harmony reexport (safe) */ __webpack_require__.d(__webpack_exports__, "DatepickerConfig", function() { return _datepicker_config__WEBPACK_IMPORTED_MODULE_6__["DatepickerConfig"]; });

/* harmony import */ var _bs_datepicker_module__WEBPACK_IMPORTED_MODULE_7__ = __webpack_require__(/*! ./bs-datepicker.module */ "./node_modules/ngx-bootstrap/datepicker/bs-datepicker.module.js");
/* harmony reexport (safe) */ __webpack_require__.d(__webpack_exports__, "BsDatepickerModule", function() { return _bs_datepicker_module__WEBPACK_IMPORTED_MODULE_7__["BsDatepickerModule"]; });

/* harmony import */ var _bs_datepicker_component__WEBPACK_IMPORTED_MODULE_8__ = __webpack_require__(/*! ./bs-datepicker.component */ "./node_modules/ngx-bootstrap/datepicker/bs-datepicker.component.js");
/* harmony reexport (safe) */ __webpack_require__.d(__webpack_exports__, "BsDatepickerDirective", function() { return _bs_datepicker_component__WEBPACK_IMPORTED_MODULE_8__["BsDatepickerDirective"]; });

/* harmony import */ var _bs_daterangepicker_component__WEBPACK_IMPORTED_MODULE_9__ = __webpack_require__(/*! ./bs-daterangepicker.component */ "./node_modules/ngx-bootstrap/datepicker/bs-daterangepicker.component.js");
/* harmony reexport (safe) */ __webpack_require__.d(__webpack_exports__, "BsDaterangepickerDirective", function() { return _bs_daterangepicker_component__WEBPACK_IMPORTED_MODULE_9__["BsDaterangepickerDirective"]; });

/* harmony import */ var _bs_datepicker_config__WEBPACK_IMPORTED_MODULE_10__ = __webpack_require__(/*! ./bs-datepicker.config */ "./node_modules/ngx-bootstrap/datepicker/bs-datepicker.config.js");
/* harmony reexport (safe) */ __webpack_require__.d(__webpack_exports__, "BsDatepickerConfig", function() { return _bs_datepicker_config__WEBPACK_IMPORTED_MODULE_10__["BsDatepickerConfig"]; });

/* harmony import */ var _bs_daterangepicker_config__WEBPACK_IMPORTED_MODULE_11__ = __webpack_require__(/*! ./bs-daterangepicker.config */ "./node_modules/ngx-bootstrap/datepicker/bs-daterangepicker.config.js");
/* harmony reexport (safe) */ __webpack_require__.d(__webpack_exports__, "BsDaterangepickerConfig", function() { return _bs_daterangepicker_config__WEBPACK_IMPORTED_MODULE_11__["BsDaterangepickerConfig"]; });

/* harmony import */ var _bs_locale_service__WEBPACK_IMPORTED_MODULE_12__ = __webpack_require__(/*! ./bs-locale.service */ "./node_modules/ngx-bootstrap/datepicker/bs-locale.service.js");
/* harmony reexport (safe) */ __webpack_require__.d(__webpack_exports__, "BsLocaleService", function() { return _bs_locale_service__WEBPACK_IMPORTED_MODULE_12__["BsLocaleService"]; });














//# sourceMappingURL=index.js.map

/***/ }),

/***/ "./node_modules/ngx-bootstrap/datepicker/models/index.js":
/*!***************************************************************!*\
  !*** ./node_modules/ngx-bootstrap/datepicker/models/index.js ***!
  \***************************************************************/
/*! exports provided: BsNavigationDirection */
/***/ (function(module, __webpack_exports__, __webpack_require__) {

"use strict";
__webpack_require__.r(__webpack_exports__);
/* harmony export (binding) */ __webpack_require__.d(__webpack_exports__, "BsNavigationDirection", function() { return BsNavigationDirection; });
/** *************** */
// events
/** *************** */
/** *************** */
// events
/** *************** */
var BsNavigationDirection;
/** *************** */
// events
/** *************** */
(function (BsNavigationDirection) {
    BsNavigationDirection[BsNavigationDirection["UP"] = 0] = "UP";
    BsNavigationDirection[BsNavigationDirection["DOWN"] = 1] = "DOWN";
})(BsNavigationDirection || (BsNavigationDirection = {}));
//# sourceMappingURL=index.js.map

/***/ }),

/***/ "./node_modules/ngx-bootstrap/datepicker/monthpicker.component.js":
/*!************************************************************************!*\
  !*** ./node_modules/ngx-bootstrap/datepicker/monthpicker.component.js ***!
  \************************************************************************/
/*! exports provided: MonthPickerComponent */
/***/ (function(module, __webpack_exports__, __webpack_require__) {

"use strict";
__webpack_require__.r(__webpack_exports__);
/* harmony export (binding) */ __webpack_require__.d(__webpack_exports__, "MonthPickerComponent", function() { return MonthPickerComponent; });
/* harmony import */ var _angular_core__WEBPACK_IMPORTED_MODULE_0__ = __webpack_require__(/*! @angular/core */ "./node_modules/@angular/core/fesm5/core.js");
/* harmony import */ var _utils_theme_provider__WEBPACK_IMPORTED_MODULE_1__ = __webpack_require__(/*! ../utils/theme-provider */ "./node_modules/ngx-bootstrap/utils/theme-provider.js");
/* harmony import */ var _datepicker_inner_component__WEBPACK_IMPORTED_MODULE_2__ = __webpack_require__(/*! ./datepicker-inner.component */ "./node_modules/ngx-bootstrap/datepicker/datepicker-inner.component.js");



var MonthPickerComponent = /** @class */ (function () {
    function MonthPickerComponent(datePicker) {
        this.rows = [];
        this.datePicker = datePicker;
    }
    Object.defineProperty(MonthPickerComponent.prototype, "isBs4", {
        get: function () {
            return !Object(_utils_theme_provider__WEBPACK_IMPORTED_MODULE_1__["isBs3"])();
        },
        enumerable: true,
        configurable: true
    });
    MonthPickerComponent.prototype.ngOnInit = function () {
        var self = this;
        this.datePicker.stepMonth = { years: 1 };
        this.datePicker.setRefreshViewHandler(function () {
            var months = new Array(12);
            var year = this.activeDate.getFullYear();
            var date;
            for (var i = 0; i < 12; i++) {
                date = new Date(year, i, 1);
                date = this.fixTimeZone(date);
                months[i] = this.createDateObject(date, this.formatMonth);
                months[i].uid = this.uniqueId + '-' + i;
            }
            self.title = this.dateFilter(this.activeDate, this.formatMonthTitle);
            self.rows = this.split(months, self.datePicker.monthColLimit);
        }, 'month');
        this.datePicker.setCompareHandler(function (date1, date2) {
            var d1 = new Date(date1.getFullYear(), date1.getMonth());
            var d2 = new Date(date2.getFullYear(), date2.getMonth());
            return d1.getTime() - d2.getTime();
        }, 'month');
        this.datePicker.refreshView();
    };
    MonthPickerComponent.decorators = [
        { type: _angular_core__WEBPACK_IMPORTED_MODULE_0__["Component"], args: [{
                    selector: 'monthpicker',
                    template: "\n<table *ngIf=\"datePicker.datepickerMode==='month'\" role=\"grid\">\n  <thead>\n    <tr>\n      <th>\n        <button type=\"button\" class=\"btn btn-default btn-sm pull-left float-left\"\n                (click)=\"datePicker.move(-1)\" tabindex=\"-1\">\u2039</button></th>\n      <th [attr.colspan]=\"((datePicker.monthColLimit - 2) <= 0) ? 1 : datePicker.monthColLimit - 2\">\n        <button [id]=\"datePicker.uniqueId + '-title'\"\n                type=\"button\" class=\"btn btn-default btn-sm\"\n                (click)=\"datePicker.toggleMode(0)\"\n                [disabled]=\"datePicker.datepickerMode === maxMode\"\n                [ngClass]=\"{disabled: datePicker.datepickerMode === maxMode}\" tabindex=\"-1\" style=\"width:100%;\">\n          <strong>{{ title }}</strong> \n        </button>\n      </th>\n      <th>\n        <button type=\"button\" class=\"btn btn-default btn-sm pull-right float-right\"\n                (click)=\"datePicker.move(1)\" tabindex=\"-1\">\u203A</button>\n      </th>\n    </tr>\n  </thead>\n  <tbody>\n    <tr *ngFor=\"let rowz of rows\">\n      <td *ngFor=\"let dtz of rowz\" class=\"text-center\" role=\"gridcell\" [attr.id]=\"dtz.uid\" [ngClass]=\"dtz.customClass\">\n        <button type=\"button\" style=\"min-width:100%;\" class=\"btn btn-default\"\n                [ngClass]=\"{'btn-link': isBs4 && !dtz.selected && !datePicker.isActive(dtz), 'btn-info': dtz.selected || (isBs4 && !dtz.selected && datePicker.isActive(dtz)), disabled: dtz.disabled, active: !isBs4 && datePicker.isActive(dtz)}\"\n                [disabled]=\"dtz.disabled\"\n                (click)=\"datePicker.select(dtz.date)\" tabindex=\"-1\">\n          <span [ngClass]=\"{'text-success': isBs4 && dtz.current, 'text-info': !isBs4 && dtz.current}\">{{ dtz.label }}</span>\n        </button>\n      </td>\n    </tr>\n  </tbody>\n</table>\n  ",
                    styles: [
                        "\n    :host .btn-info .text-success {\n      color: #fff !important;\n    }\n  "
                    ]
                },] },
    ];
    // todo: key events implementation
    /** @nocollapse */
    MonthPickerComponent.ctorParameters = function () { return [
        { type: _datepicker_inner_component__WEBPACK_IMPORTED_MODULE_2__["DatePickerInnerComponent"], },
    ]; };
    return MonthPickerComponent;
}());

//# sourceMappingURL=monthpicker.component.js.map

/***/ }),

/***/ "./node_modules/ngx-bootstrap/datepicker/reducer/_defaults.js":
/*!********************************************************************!*\
  !*** ./node_modules/ngx-bootstrap/datepicker/reducer/_defaults.js ***!
  \********************************************************************/
/*! exports provided: defaultMonthOptions */
/***/ (function(module, __webpack_exports__, __webpack_require__) {

"use strict";
__webpack_require__.r(__webpack_exports__);
/* harmony export (binding) */ __webpack_require__.d(__webpack_exports__, "defaultMonthOptions", function() { return defaultMonthOptions; });
var defaultMonthOptions = {
    width: 7,
    height: 6
};
//# sourceMappingURL=_defaults.js.map

/***/ }),

/***/ "./node_modules/ngx-bootstrap/datepicker/reducer/bs-datepicker.actions.js":
/*!********************************************************************************!*\
  !*** ./node_modules/ngx-bootstrap/datepicker/reducer/bs-datepicker.actions.js ***!
  \********************************************************************************/
/*! exports provided: BsDatepickerActions */
/***/ (function(module, __webpack_exports__, __webpack_require__) {

"use strict";
__webpack_require__.r(__webpack_exports__);
/* harmony export (binding) */ __webpack_require__.d(__webpack_exports__, "BsDatepickerActions", function() { return BsDatepickerActions; });
/* harmony import */ var _angular_core__WEBPACK_IMPORTED_MODULE_0__ = __webpack_require__(/*! @angular/core */ "./node_modules/@angular/core/fesm5/core.js");

var BsDatepickerActions = /** @class */ (function () {
    function BsDatepickerActions() {
    }
    BsDatepickerActions.prototype.calculate = function () {
        return { type: BsDatepickerActions.CALCULATE };
    };
    BsDatepickerActions.prototype.format = function () {
        return { type: BsDatepickerActions.FORMAT };
    };
    BsDatepickerActions.prototype.flag = function () {
        return { type: BsDatepickerActions.FLAG };
    };
    BsDatepickerActions.prototype.select = function (date) {
        return {
            type: BsDatepickerActions.SELECT,
            payload: date
        };
    };
    BsDatepickerActions.prototype.changeViewMode = function (event) {
        return {
            type: BsDatepickerActions.CHANGE_VIEWMODE,
            payload: event
        };
    };
    BsDatepickerActions.prototype.navigateTo = function (event) {
        return {
            type: BsDatepickerActions.NAVIGATE_TO,
            payload: event
        };
    };
    BsDatepickerActions.prototype.navigateStep = function (step) {
        return {
            type: BsDatepickerActions.NAVIGATE_OFFSET,
            payload: step
        };
    };
    BsDatepickerActions.prototype.setOptions = function (options) {
        return {
            type: BsDatepickerActions.SET_OPTIONS,
            payload: options
        };
    };
    // date range picker
    // date range picker
    BsDatepickerActions.prototype.selectRange = 
    // date range picker
    function (value) {
        return {
            type: BsDatepickerActions.SELECT_RANGE,
            payload: value
        };
    };
    BsDatepickerActions.prototype.hoverDay = function (event) {
        return {
            type: BsDatepickerActions.HOVER,
            payload: event.isHovered ? event.cell.date : null
        };
    };
    BsDatepickerActions.prototype.minDate = function (date) {
        return {
            type: BsDatepickerActions.SET_MIN_DATE,
            payload: date
        };
    };
    BsDatepickerActions.prototype.maxDate = function (date) {
        return {
            type: BsDatepickerActions.SET_MAX_DATE,
            payload: date
        };
    };
    BsDatepickerActions.prototype.isDisabled = function (value) {
        return {
            type: BsDatepickerActions.SET_IS_DISABLED,
            payload: value
        };
    };
    BsDatepickerActions.prototype.setLocale = function (locale) {
        return {
            type: BsDatepickerActions.SET_LOCALE,
            payload: locale
        };
    };
    BsDatepickerActions.CALCULATE = '[datepicker] calculate dates matrix';
    BsDatepickerActions.FORMAT = '[datepicker] format datepicker values';
    BsDatepickerActions.FLAG = '[datepicker] set flags';
    BsDatepickerActions.SELECT = '[datepicker] select date';
    BsDatepickerActions.NAVIGATE_OFFSET = '[datepicker] shift view date';
    BsDatepickerActions.NAVIGATE_TO = '[datepicker] change view date';
    BsDatepickerActions.SET_OPTIONS = '[datepicker] update render options';
    BsDatepickerActions.HOVER = '[datepicker] hover date';
    BsDatepickerActions.CHANGE_VIEWMODE = '[datepicker] switch view mode';
    BsDatepickerActions.SET_MIN_DATE = '[datepicker] set min date';
    BsDatepickerActions.SET_MAX_DATE = '[datepicker] set max date';
    BsDatepickerActions.SET_IS_DISABLED = '[datepicker] set is disabled';
    BsDatepickerActions.SET_LOCALE = '[datepicker] set datepicker locale';
    BsDatepickerActions.SELECT_RANGE = '[daterangepicker] select dates range';
    BsDatepickerActions.decorators = [
        { type: _angular_core__WEBPACK_IMPORTED_MODULE_0__["Injectable"] },
    ];
    return BsDatepickerActions;
}());

//# sourceMappingURL=bs-datepicker.actions.js.map

/***/ }),

/***/ "./node_modules/ngx-bootstrap/datepicker/reducer/bs-datepicker.effects.js":
/*!********************************************************************************!*\
  !*** ./node_modules/ngx-bootstrap/datepicker/reducer/bs-datepicker.effects.js ***!
  \********************************************************************************/
/*! exports provided: BsDatepickerEffects */
/***/ (function(module, __webpack_exports__, __webpack_require__) {

"use strict";
__webpack_require__.r(__webpack_exports__);
/* harmony export (binding) */ __webpack_require__.d(__webpack_exports__, "BsDatepickerEffects", function() { return BsDatepickerEffects; });
/* harmony import */ var _angular_core__WEBPACK_IMPORTED_MODULE_0__ = __webpack_require__(/*! @angular/core */ "./node_modules/@angular/core/fesm5/core.js");
/* harmony import */ var _chronos_utils_date_getters__WEBPACK_IMPORTED_MODULE_1__ = __webpack_require__(/*! ../../chronos/utils/date-getters */ "./node_modules/ngx-bootstrap/chronos/utils/date-getters.js");
/* harmony import */ var _bs_datepicker_actions__WEBPACK_IMPORTED_MODULE_2__ = __webpack_require__(/*! ./bs-datepicker.actions */ "./node_modules/ngx-bootstrap/datepicker/reducer/bs-datepicker.actions.js");
/* harmony import */ var _bs_locale_service__WEBPACK_IMPORTED_MODULE_3__ = __webpack_require__(/*! ../bs-locale.service */ "./node_modules/ngx-bootstrap/datepicker/bs-locale.service.js");
/* harmony import */ var rxjs_operators__WEBPACK_IMPORTED_MODULE_4__ = __webpack_require__(/*! rxjs/operators */ "./node_modules/rxjs/_esm5/operators/index.js");





var BsDatepickerEffects = /** @class */ (function () {
    function BsDatepickerEffects(_actions, _localeService) {
        this._actions = _actions;
        this._localeService = _localeService;
        this._subs = [];
    }
    BsDatepickerEffects.prototype.init = function (_bsDatepickerStore) {
        this._store = _bsDatepickerStore;
        return this;
    };
    /** setters */
    /** setters */
    BsDatepickerEffects.prototype.setValue = /** setters */
    function (value) {
        this._store.dispatch(this._actions.select(value));
    };
    BsDatepickerEffects.prototype.setRangeValue = function (value) {
        this._store.dispatch(this._actions.selectRange(value));
    };
    BsDatepickerEffects.prototype.setMinDate = function (value) {
        this._store.dispatch(this._actions.minDate(value));
        return this;
    };
    BsDatepickerEffects.prototype.setMaxDate = function (value) {
        this._store.dispatch(this._actions.maxDate(value));
        return this;
    };
    BsDatepickerEffects.prototype.setDisabled = function (value) {
        this._store.dispatch(this._actions.isDisabled(value));
        return this;
    };
    /* Set rendering options */
    /* Set rendering options */
    BsDatepickerEffects.prototype.setOptions = /* Set rendering options */
    function (_config) {
        var _options = Object.assign({ locale: this._localeService.currentLocale }, _config);
        this._store.dispatch(this._actions.setOptions(_options));
        return this;
    };
    /** view to mode bindings */
    /** view to mode bindings */
    BsDatepickerEffects.prototype.setBindings = /** view to mode bindings */
    function (container) {
        container.daysCalendar = this._store
            .select(function (state) { return state.flaggedMonths; })
            .pipe(Object(rxjs_operators__WEBPACK_IMPORTED_MODULE_4__["filter"])(function (months) { return !!months; }));
        // month calendar
        container.monthsCalendar = this._store
            .select(function (state) { return state.flaggedMonthsCalendar; })
            .pipe(Object(rxjs_operators__WEBPACK_IMPORTED_MODULE_4__["filter"])(function (months) { return !!months; }));
        // year calendar
        container.yearsCalendar = this._store
            .select(function (state) { return state.yearsCalendarFlagged; })
            .pipe(Object(rxjs_operators__WEBPACK_IMPORTED_MODULE_4__["filter"])(function (years) { return !!years; }));
        container.viewMode = this._store.select(function (state) { return state.view.mode; });
        container.options = this._store
            .select(function (state) { return state.showWeekNumbers; })
            .pipe(Object(rxjs_operators__WEBPACK_IMPORTED_MODULE_4__["map"])(function (showWeekNumbers) { return ({ showWeekNumbers: showWeekNumbers }); }));
        return this;
    };
    /** event handlers */
    /** event handlers */
    BsDatepickerEffects.prototype.setEventHandlers = /** event handlers */
    function (container) {
        var _this = this;
        container.setViewMode = function (event) {
            _this._store.dispatch(_this._actions.changeViewMode(event));
        };
        container.navigateTo = function (event) {
            _this._store.dispatch(_this._actions.navigateStep(event.step));
        };
        container.dayHoverHandler = function (event) {
            var _cell = event.cell;
            if (_cell.isOtherMonth || _cell.isDisabled) {
                return;
            }
            _this._store.dispatch(_this._actions.hoverDay(event));
            _cell.isHovered = event.isHovered;
        };
        container.monthHoverHandler = function (event) {
            event.cell.isHovered = event.isHovered;
        };
        container.yearHoverHandler = function (event) {
            event.cell.isHovered = event.isHovered;
        };
        /** select handlers */
        // container.daySelectHandler = (day: DayViewModel): void => {
        //   if (day.isOtherMonth || day.isDisabled) {
        //     return;
        //   }
        //   this._store.dispatch(this._actions.select(day.date));
        // };
        container.monthSelectHandler = function (event) {
            if (event.isDisabled) {
                return;
            }
            _this._store.dispatch(_this._actions.navigateTo({
                unit: { month: Object(_chronos_utils_date_getters__WEBPACK_IMPORTED_MODULE_1__["getMonth"])(event.date) },
                viewMode: 'day'
            }));
        };
        container.yearSelectHandler = function (event) {
            if (event.isDisabled) {
                return;
            }
            _this._store.dispatch(_this._actions.navigateTo({
                unit: { year: Object(_chronos_utils_date_getters__WEBPACK_IMPORTED_MODULE_1__["getFullYear"])(event.date) },
                viewMode: 'month'
            }));
        };
        return this;
    };
    BsDatepickerEffects.prototype.registerDatepickerSideEffects = function () {
        var _this = this;
        this._subs.push(this._store.select(function (state) { return state.view; }).subscribe(function (view) {
            _this._store.dispatch(_this._actions.calculate());
        }));
        // format calendar values on month model change
        this._subs.push(this._store
            .select(function (state) { return state.monthsModel; })
            .pipe(Object(rxjs_operators__WEBPACK_IMPORTED_MODULE_4__["filter"])(function (monthModel) { return !!monthModel; }))
            .subscribe(function (month) { return _this._store.dispatch(_this._actions.format()); }));
        // flag day values
        this._subs.push(this._store
            .select(function (state) { return state.formattedMonths; })
            .pipe(Object(rxjs_operators__WEBPACK_IMPORTED_MODULE_4__["filter"])(function (month) { return !!month; }))
            .subscribe(function (month) { return _this._store.dispatch(_this._actions.flag()); }));
        // flag day values
        this._subs.push(this._store
            .select(function (state) { return state.selectedDate; })
            .pipe(Object(rxjs_operators__WEBPACK_IMPORTED_MODULE_4__["filter"])(function (selectedDate) { return !!selectedDate; }))
            .subscribe(function (selectedDate) { return _this._store.dispatch(_this._actions.flag()); }));
        // flag for date range picker
        this._subs.push(this._store
            .select(function (state) { return state.selectedRange; })
            .pipe(Object(rxjs_operators__WEBPACK_IMPORTED_MODULE_4__["filter"])(function (selectedRange) { return !!selectedRange; }))
            .subscribe(function (selectedRange) { return _this._store.dispatch(_this._actions.flag()); }));
        // monthsCalendar
        this._subs.push(this._store
            .select(function (state) { return state.monthsCalendar; })
            .subscribe(function () { return _this._store.dispatch(_this._actions.flag()); }));
        // years calendar
        this._subs.push(this._store
            .select(function (state) { return state.yearsCalendarModel; })
            .pipe(Object(rxjs_operators__WEBPACK_IMPORTED_MODULE_4__["filter"])(function (state) { return !!state; }))
            .subscribe(function () { return _this._store.dispatch(_this._actions.flag()); }));
        // on hover
        this._subs.push(this._store
            .select(function (state) { return state.hoveredDate; })
            .pipe(Object(rxjs_operators__WEBPACK_IMPORTED_MODULE_4__["filter"])(function (hoveredDate) { return !!hoveredDate; }))
            .subscribe(function (hoveredDate) { return _this._store.dispatch(_this._actions.flag()); }));
        // on locale change
        this._subs.push(this._localeService.localeChange
            .subscribe(function (locale) { return _this._store.dispatch(_this._actions.setLocale(locale)); }));
        return this;
    };
    BsDatepickerEffects.prototype.destroy = function () {
        for (var _i = 0, _a = this._subs; _i < _a.length; _i++) {
            var sub = _a[_i];
            sub.unsubscribe();
        }
    };
    BsDatepickerEffects.decorators = [
        { type: _angular_core__WEBPACK_IMPORTED_MODULE_0__["Injectable"] },
    ];
    /** @nocollapse */
    BsDatepickerEffects.ctorParameters = function () { return [
        { type: _bs_datepicker_actions__WEBPACK_IMPORTED_MODULE_2__["BsDatepickerActions"], },
        { type: _bs_locale_service__WEBPACK_IMPORTED_MODULE_3__["BsLocaleService"], },
    ]; };
    return BsDatepickerEffects;
}());

//# sourceMappingURL=bs-datepicker.effects.js.map

/***/ }),

/***/ "./node_modules/ngx-bootstrap/datepicker/reducer/bs-datepicker.reducer.js":
/*!********************************************************************************!*\
  !*** ./node_modules/ngx-bootstrap/datepicker/reducer/bs-datepicker.reducer.js ***!
  \********************************************************************************/
/*! exports provided: bsDatepickerReducer */
/***/ (function(module, __webpack_exports__, __webpack_require__) {

"use strict";
__webpack_require__.r(__webpack_exports__);
/* harmony export (binding) */ __webpack_require__.d(__webpack_exports__, "bsDatepickerReducer", function() { return bsDatepickerReducer; });
/* harmony import */ var _bs_datepicker_state__WEBPACK_IMPORTED_MODULE_0__ = __webpack_require__(/*! ./bs-datepicker.state */ "./node_modules/ngx-bootstrap/datepicker/reducer/bs-datepicker.state.js");
/* harmony import */ var _bs_datepicker_actions__WEBPACK_IMPORTED_MODULE_1__ = __webpack_require__(/*! ./bs-datepicker.actions */ "./node_modules/ngx-bootstrap/datepicker/reducer/bs-datepicker.actions.js");
/* harmony import */ var _engine_calc_days_calendar__WEBPACK_IMPORTED_MODULE_2__ = __webpack_require__(/*! ../engine/calc-days-calendar */ "./node_modules/ngx-bootstrap/datepicker/engine/calc-days-calendar.js");
/* harmony import */ var _engine_format_days_calendar__WEBPACK_IMPORTED_MODULE_3__ = __webpack_require__(/*! ../engine/format-days-calendar */ "./node_modules/ngx-bootstrap/datepicker/engine/format-days-calendar.js");
/* harmony import */ var _engine_flag_days_calendar__WEBPACK_IMPORTED_MODULE_4__ = __webpack_require__(/*! ../engine/flag-days-calendar */ "./node_modules/ngx-bootstrap/datepicker/engine/flag-days-calendar.js");
/* harmony import */ var _chronos_utils_date_setters__WEBPACK_IMPORTED_MODULE_5__ = __webpack_require__(/*! ../../chronos/utils/date-setters */ "./node_modules/ngx-bootstrap/chronos/utils/date-setters.js");
/* harmony import */ var _engine_view_mode__WEBPACK_IMPORTED_MODULE_6__ = __webpack_require__(/*! ../engine/view-mode */ "./node_modules/ngx-bootstrap/datepicker/engine/view-mode.js");
/* harmony import */ var _engine_format_months_calendar__WEBPACK_IMPORTED_MODULE_7__ = __webpack_require__(/*! ../engine/format-months-calendar */ "./node_modules/ngx-bootstrap/datepicker/engine/format-months-calendar.js");
/* harmony import */ var _engine_flag_months_calendar__WEBPACK_IMPORTED_MODULE_8__ = __webpack_require__(/*! ../engine/flag-months-calendar */ "./node_modules/ngx-bootstrap/datepicker/engine/flag-months-calendar.js");
/* harmony import */ var _engine_format_years_calendar__WEBPACK_IMPORTED_MODULE_9__ = __webpack_require__(/*! ../engine/format-years-calendar */ "./node_modules/ngx-bootstrap/datepicker/engine/format-years-calendar.js");
/* harmony import */ var _engine_flag_years_calendar__WEBPACK_IMPORTED_MODULE_10__ = __webpack_require__(/*! ../engine/flag-years-calendar */ "./node_modules/ngx-bootstrap/datepicker/engine/flag-years-calendar.js");
/* harmony import */ var _chronos_utils_type_checks__WEBPACK_IMPORTED_MODULE_11__ = __webpack_require__(/*! ../../chronos/utils/type-checks */ "./node_modules/ngx-bootstrap/chronos/utils/type-checks.js");
/* harmony import */ var _chronos_utils_start_end_of__WEBPACK_IMPORTED_MODULE_12__ = __webpack_require__(/*! ../../chronos/utils/start-end-of */ "./node_modules/ngx-bootstrap/chronos/utils/start-end-of.js");
/* harmony import */ var _chronos_locale_locales__WEBPACK_IMPORTED_MODULE_13__ = __webpack_require__(/*! ../../chronos/locale/locales */ "./node_modules/ngx-bootstrap/chronos/locale/locales.js");
/* harmony import */ var _chronos_utils_date_compare__WEBPACK_IMPORTED_MODULE_14__ = __webpack_require__(/*! ../../chronos/utils/date-compare */ "./node_modules/ngx-bootstrap/chronos/utils/date-compare.js");















function bsDatepickerReducer(state, action) {
    if (state === void 0) { state = _bs_datepicker_state__WEBPACK_IMPORTED_MODULE_0__["initialDatepickerState"]; }
    switch (action.type) {
        case _bs_datepicker_actions__WEBPACK_IMPORTED_MODULE_1__["BsDatepickerActions"].CALCULATE: {
            return calculateReducer(state);
        }
        case _bs_datepicker_actions__WEBPACK_IMPORTED_MODULE_1__["BsDatepickerActions"].FORMAT: {
            return formatReducer(state, action);
        }
        case _bs_datepicker_actions__WEBPACK_IMPORTED_MODULE_1__["BsDatepickerActions"].FLAG: {
            return flagReducer(state, action);
        }
        case _bs_datepicker_actions__WEBPACK_IMPORTED_MODULE_1__["BsDatepickerActions"].NAVIGATE_OFFSET: {
            var date = Object(_chronos_utils_date_setters__WEBPACK_IMPORTED_MODULE_5__["shiftDate"])(Object(_chronos_utils_start_end_of__WEBPACK_IMPORTED_MODULE_12__["startOf"])(state.view.date, 'month'), action.payload);
            var newState = {
                view: {
                    mode: state.view.mode,
                    date: date
                }
            };
            return Object.assign({}, state, newState);
        }
        case _bs_datepicker_actions__WEBPACK_IMPORTED_MODULE_1__["BsDatepickerActions"].NAVIGATE_TO: {
            var payload = action.payload;
            var date = Object(_chronos_utils_date_setters__WEBPACK_IMPORTED_MODULE_5__["setFullDate"])(state.view.date, payload.unit);
            var mode = payload.viewMode;
            var newState = { view: { date: date, mode: mode } };
            return Object.assign({}, state, newState);
        }
        case _bs_datepicker_actions__WEBPACK_IMPORTED_MODULE_1__["BsDatepickerActions"].CHANGE_VIEWMODE: {
            if (!Object(_engine_view_mode__WEBPACK_IMPORTED_MODULE_6__["canSwitchMode"])(action.payload)) {
                return state;
            }
            var date = state.view.date;
            var mode = action.payload;
            var newState = { view: { date: date, mode: mode } };
            return Object.assign({}, state, newState);
        }
        case _bs_datepicker_actions__WEBPACK_IMPORTED_MODULE_1__["BsDatepickerActions"].HOVER: {
            return Object.assign({}, state, { hoveredDate: action.payload });
        }
        case _bs_datepicker_actions__WEBPACK_IMPORTED_MODULE_1__["BsDatepickerActions"].SELECT: {
            var newState = {
                selectedDate: action.payload,
                view: state.view
            };
            var mode = state.view.mode;
            var _date = action.payload || state.view.date;
            var date = getViewDate(_date, state.minDate, state.maxDate);
            newState.view = { mode: mode, date: date };
            return Object.assign({}, state, newState);
        }
        case _bs_datepicker_actions__WEBPACK_IMPORTED_MODULE_1__["BsDatepickerActions"].SET_OPTIONS: {
            var newState = action.payload;
            // preserve view mode
            var mode = state.view.mode;
            var _viewDate = Object(_chronos_utils_type_checks__WEBPACK_IMPORTED_MODULE_11__["isDateValid"])(newState.value) && newState.value
                || Object(_chronos_utils_type_checks__WEBPACK_IMPORTED_MODULE_11__["isArray"])(newState.value) && Object(_chronos_utils_type_checks__WEBPACK_IMPORTED_MODULE_11__["isDateValid"])(newState.value[0]) && newState.value[0]
                || state.view.date;
            var date = getViewDate(_viewDate, newState.minDate, newState.maxDate);
            newState.view = { mode: mode, date: date };
            // update selected value
            if (newState.value) {
                // if new value is array we work with date range
                if (Object(_chronos_utils_type_checks__WEBPACK_IMPORTED_MODULE_11__["isArray"])(newState.value)) {
                    newState.selectedRange = newState.value;
                }
                // if new value is a date -> datepicker
                if (newState.value instanceof Date) {
                    newState.selectedDate = newState.value;
                }
                // provided value is not supported :)
                // need to report it somehow
            }
            return Object.assign({}, state, newState);
        }
        // date range picker
        case _bs_datepicker_actions__WEBPACK_IMPORTED_MODULE_1__["BsDatepickerActions"].SELECT_RANGE: {
            var newState = {
                selectedRange: action.payload,
                view: state.view
            };
            var mode = state.view.mode;
            var _date = action.payload && action.payload[0] || state.view.date;
            var date = getViewDate(_date, state.minDate, state.maxDate);
            newState.view = { mode: mode, date: date };
            return Object.assign({}, state, newState);
        }
        case _bs_datepicker_actions__WEBPACK_IMPORTED_MODULE_1__["BsDatepickerActions"].SET_MIN_DATE: {
            return Object.assign({}, state, {
                minDate: action.payload
            });
        }
        case _bs_datepicker_actions__WEBPACK_IMPORTED_MODULE_1__["BsDatepickerActions"].SET_MAX_DATE: {
            return Object.assign({}, state, {
                maxDate: action.payload
            });
        }
        case _bs_datepicker_actions__WEBPACK_IMPORTED_MODULE_1__["BsDatepickerActions"].SET_IS_DISABLED: {
            return Object.assign({}, state, {
                isDisabled: action.payload
            });
        }
        default:
            return state;
    }
}
function calculateReducer(state) {
    // how many calendars
    var displayMonths = state.displayMonths;
    // use selected date on initial rendering if set
    var viewDate = state.view.date;
    if (state.view.mode === 'day') {
        state.monthViewOptions.firstDayOfWeek = Object(_chronos_locale_locales__WEBPACK_IMPORTED_MODULE_13__["getLocale"])(state.locale).firstDayOfWeek();
        var monthsModel = new Array(displayMonths);
        for (var monthIndex = 0; monthIndex < displayMonths; monthIndex++) {
            // todo: for unlinked calendars it will be harder
            monthsModel[monthIndex] = Object(_engine_calc_days_calendar__WEBPACK_IMPORTED_MODULE_2__["calcDaysCalendar"])(viewDate, state.monthViewOptions);
            viewDate = Object(_chronos_utils_date_setters__WEBPACK_IMPORTED_MODULE_5__["shiftDate"])(viewDate, { month: 1 });
        }
        return Object.assign({}, state, { monthsModel: monthsModel });
    }
    if (state.view.mode === 'month') {
        var monthsCalendar = new Array(displayMonths);
        for (var calendarIndex = 0; calendarIndex < displayMonths; calendarIndex++) {
            // todo: for unlinked calendars it will be harder
            monthsCalendar[calendarIndex] = Object(_engine_format_months_calendar__WEBPACK_IMPORTED_MODULE_7__["formatMonthsCalendar"])(viewDate, getFormatOptions(state));
            viewDate = Object(_chronos_utils_date_setters__WEBPACK_IMPORTED_MODULE_5__["shiftDate"])(viewDate, { year: 1 });
        }
        return Object.assign({}, state, { monthsCalendar: monthsCalendar });
    }
    if (state.view.mode === 'year') {
        var yearsCalendarModel = new Array(displayMonths);
        for (var calendarIndex = 0; calendarIndex < displayMonths; calendarIndex++) {
            // todo: for unlinked calendars it will be harder
            yearsCalendarModel[calendarIndex] = Object(_engine_format_years_calendar__WEBPACK_IMPORTED_MODULE_9__["formatYearsCalendar"])(viewDate, getFormatOptions(state));
            viewDate = Object(_chronos_utils_date_setters__WEBPACK_IMPORTED_MODULE_5__["shiftDate"])(viewDate, { year: _engine_format_years_calendar__WEBPACK_IMPORTED_MODULE_9__["yearsPerCalendar"] });
        }
        return Object.assign({}, state, { yearsCalendarModel: yearsCalendarModel });
    }
    return state;
}
function formatReducer(state, action) {
    if (state.view.mode === 'day') {
        var formattedMonths = state.monthsModel.map(function (month, monthIndex) {
            return Object(_engine_format_days_calendar__WEBPACK_IMPORTED_MODULE_3__["formatDaysCalendar"])(month, getFormatOptions(state), monthIndex);
        });
        return Object.assign({}, state, { formattedMonths: formattedMonths });
    }
    // how many calendars
    var displayMonths = state.displayMonths;
    // check initial rendering
    // use selected date on initial rendering if set
    var viewDate = state.view.date;
    if (state.view.mode === 'month') {
        var monthsCalendar = new Array(displayMonths);
        for (var calendarIndex = 0; calendarIndex < displayMonths; calendarIndex++) {
            // todo: for unlinked calendars it will be harder
            monthsCalendar[calendarIndex] = Object(_engine_format_months_calendar__WEBPACK_IMPORTED_MODULE_7__["formatMonthsCalendar"])(viewDate, getFormatOptions(state));
            viewDate = Object(_chronos_utils_date_setters__WEBPACK_IMPORTED_MODULE_5__["shiftDate"])(viewDate, { year: 1 });
        }
        return Object.assign({}, state, { monthsCalendar: monthsCalendar });
    }
    if (state.view.mode === 'year') {
        var yearsCalendarModel = new Array(displayMonths);
        for (var calendarIndex = 0; calendarIndex < displayMonths; calendarIndex++) {
            // todo: for unlinked calendars it will be harder
            yearsCalendarModel[calendarIndex] = Object(_engine_format_years_calendar__WEBPACK_IMPORTED_MODULE_9__["formatYearsCalendar"])(viewDate, getFormatOptions(state));
            viewDate = Object(_chronos_utils_date_setters__WEBPACK_IMPORTED_MODULE_5__["shiftDate"])(viewDate, { year: 16 });
        }
        return Object.assign({}, state, { yearsCalendarModel: yearsCalendarModel });
    }
    return state;
}
function flagReducer(state, action) {
    if (state.view.mode === 'day') {
        var flaggedMonths = state.formattedMonths.map(function (formattedMonth, monthIndex) {
            return Object(_engine_flag_days_calendar__WEBPACK_IMPORTED_MODULE_4__["flagDaysCalendar"])(formattedMonth, {
                isDisabled: state.isDisabled,
                minDate: state.minDate,
                maxDate: state.maxDate,
                hoveredDate: state.hoveredDate,
                selectedDate: state.selectedDate,
                selectedRange: state.selectedRange,
                displayMonths: state.displayMonths,
                monthIndex: monthIndex
            });
        });
        return Object.assign({}, state, { flaggedMonths: flaggedMonths });
    }
    if (state.view.mode === 'month') {
        var flaggedMonthsCalendar = state.monthsCalendar.map(function (formattedMonth, monthIndex) {
            return Object(_engine_flag_months_calendar__WEBPACK_IMPORTED_MODULE_8__["flagMonthsCalendar"])(formattedMonth, {
                isDisabled: state.isDisabled,
                minDate: state.minDate,
                maxDate: state.maxDate,
                hoveredMonth: state.hoveredMonth,
                displayMonths: state.displayMonths,
                monthIndex: monthIndex
            });
        });
        return Object.assign({}, state, { flaggedMonthsCalendar: flaggedMonthsCalendar });
    }
    if (state.view.mode === 'year') {
        var yearsCalendarFlagged = state.yearsCalendarModel.map(function (formattedMonth, yearIndex) {
            return Object(_engine_flag_years_calendar__WEBPACK_IMPORTED_MODULE_10__["flagYearsCalendar"])(formattedMonth, {
                isDisabled: state.isDisabled,
                minDate: state.minDate,
                maxDate: state.maxDate,
                hoveredYear: state.hoveredYear,
                displayMonths: state.displayMonths,
                yearIndex: yearIndex
            });
        });
        return Object.assign({}, state, { yearsCalendarFlagged: yearsCalendarFlagged });
    }
    return state;
}
function getFormatOptions(state) {
    return {
        locale: state.locale,
        monthTitle: state.monthTitle,
        yearTitle: state.yearTitle,
        dayLabel: state.dayLabel,
        monthLabel: state.monthLabel,
        yearLabel: state.yearLabel,
        weekNumbers: state.weekNumbers
    };
}
/**
 * if view date is provided (bsValue|ngModel) it should be shown
 * if view date is not provider:
 * if minDate>currentDate (default view value), show minDate
 * if maxDate<currentDate(default view value) show maxDate
 */
function getViewDate(viewDate, minDate, maxDate) {
    var _date = Array.isArray(viewDate) ? viewDate[0] : viewDate;
    if (minDate && Object(_chronos_utils_date_compare__WEBPACK_IMPORTED_MODULE_14__["isAfter"])(minDate, _date, 'day')) {
        return minDate;
    }
    if (maxDate && Object(_chronos_utils_date_compare__WEBPACK_IMPORTED_MODULE_14__["isBefore"])(maxDate, _date, 'day')) {
        return maxDate;
    }
    return _date;
}
//# sourceMappingURL=bs-datepicker.reducer.js.map

/***/ }),

/***/ "./node_modules/ngx-bootstrap/datepicker/reducer/bs-datepicker.state.js":
/*!******************************************************************************!*\
  !*** ./node_modules/ngx-bootstrap/datepicker/reducer/bs-datepicker.state.js ***!
  \******************************************************************************/
/*! exports provided: BsDatepickerState, initialDatepickerState */
/***/ (function(module, __webpack_exports__, __webpack_require__) {

"use strict";
__webpack_require__.r(__webpack_exports__);
/* harmony export (binding) */ __webpack_require__.d(__webpack_exports__, "BsDatepickerState", function() { return BsDatepickerState; });
/* harmony export (binding) */ __webpack_require__.d(__webpack_exports__, "initialDatepickerState", function() { return initialDatepickerState; });
/* harmony import */ var _defaults__WEBPACK_IMPORTED_MODULE_0__ = __webpack_require__(/*! ./_defaults */ "./node_modules/ngx-bootstrap/datepicker/reducer/_defaults.js");
/* harmony import */ var _bs_datepicker_config__WEBPACK_IMPORTED_MODULE_1__ = __webpack_require__(/*! ../bs-datepicker.config */ "./node_modules/ngx-bootstrap/datepicker/bs-datepicker.config.js");


var BsDatepickerState = /** @class */ (function () {
    function BsDatepickerState() {
    }
    return BsDatepickerState;
}());

var _initialView = { date: new Date(), mode: 'day' };
var initialDatepickerState = Object.assign(new _bs_datepicker_config__WEBPACK_IMPORTED_MODULE_1__["BsDatepickerConfig"](), {
    locale: 'en',
    view: _initialView,
    selectedRange: [],
    monthViewOptions: _defaults__WEBPACK_IMPORTED_MODULE_0__["defaultMonthOptions"]
});
//# sourceMappingURL=bs-datepicker.state.js.map

/***/ }),

/***/ "./node_modules/ngx-bootstrap/datepicker/reducer/bs-datepicker.store.js":
/*!******************************************************************************!*\
  !*** ./node_modules/ngx-bootstrap/datepicker/reducer/bs-datepicker.store.js ***!
  \******************************************************************************/
/*! exports provided: BsDatepickerStore */
/***/ (function(module, __webpack_exports__, __webpack_require__) {

"use strict";
__webpack_require__.r(__webpack_exports__);
/* harmony export (binding) */ __webpack_require__.d(__webpack_exports__, "BsDatepickerStore", function() { return BsDatepickerStore; });
/* harmony import */ var _angular_core__WEBPACK_IMPORTED_MODULE_0__ = __webpack_require__(/*! @angular/core */ "./node_modules/@angular/core/fesm5/core.js");
/* harmony import */ var _mini_ngrx_store_class__WEBPACK_IMPORTED_MODULE_1__ = __webpack_require__(/*! ../../mini-ngrx/store.class */ "./node_modules/ngx-bootstrap/mini-ngrx/store.class.js");
/* harmony import */ var _bs_datepicker_state__WEBPACK_IMPORTED_MODULE_2__ = __webpack_require__(/*! ./bs-datepicker.state */ "./node_modules/ngx-bootstrap/datepicker/reducer/bs-datepicker.state.js");
/* harmony import */ var rxjs__WEBPACK_IMPORTED_MODULE_3__ = __webpack_require__(/*! rxjs */ "./node_modules/rxjs/_esm5/index.js");
/* harmony import */ var _mini_ngrx_state_class__WEBPACK_IMPORTED_MODULE_4__ = __webpack_require__(/*! ../../mini-ngrx/state.class */ "./node_modules/ngx-bootstrap/mini-ngrx/state.class.js");
/* harmony import */ var _bs_datepicker_reducer__WEBPACK_IMPORTED_MODULE_5__ = __webpack_require__(/*! ./bs-datepicker.reducer */ "./node_modules/ngx-bootstrap/datepicker/reducer/bs-datepicker.reducer.js");
var __extends = (undefined && undefined.__extends) || (function () {
    var extendStatics = Object.setPrototypeOf ||
        ({ __proto__: [] } instanceof Array && function (d, b) { d.__proto__ = b; }) ||
        function (d, b) { for (var p in b) if (b.hasOwnProperty(p)) d[p] = b[p]; };
    return function (d, b) {
        extendStatics(d, b);
        function __() { this.constructor = d; }
        d.prototype = b === null ? Object.create(b) : (__.prototype = b.prototype, new __());
    };
})();






var BsDatepickerStore = /** @class */ (function (_super) {
    __extends(BsDatepickerStore, _super);
    function BsDatepickerStore() {
        var _this = this;
        var _dispatcher = new rxjs__WEBPACK_IMPORTED_MODULE_3__["BehaviorSubject"]({
            type: '[datepicker] dispatcher init'
        });
        var state = new _mini_ngrx_state_class__WEBPACK_IMPORTED_MODULE_4__["MiniState"](_bs_datepicker_state__WEBPACK_IMPORTED_MODULE_2__["initialDatepickerState"], _dispatcher, _bs_datepicker_reducer__WEBPACK_IMPORTED_MODULE_5__["bsDatepickerReducer"]);
        _this = _super.call(this, _dispatcher, _bs_datepicker_reducer__WEBPACK_IMPORTED_MODULE_5__["bsDatepickerReducer"], state) || this;
        return _this;
    }
    BsDatepickerStore.decorators = [
        { type: _angular_core__WEBPACK_IMPORTED_MODULE_0__["Injectable"] },
    ];
    /** @nocollapse */
    BsDatepickerStore.ctorParameters = function () { return []; };
    return BsDatepickerStore;
}(_mini_ngrx_store_class__WEBPACK_IMPORTED_MODULE_1__["MiniStore"]));

//# sourceMappingURL=bs-datepicker.store.js.map

/***/ }),

/***/ "./node_modules/ngx-bootstrap/datepicker/themes/bs/bs-calendar-layout.component.js":
/*!*****************************************************************************************!*\
  !*** ./node_modules/ngx-bootstrap/datepicker/themes/bs/bs-calendar-layout.component.js ***!
  \*****************************************************************************************/
/*! exports provided: BsCalendarLayoutComponent */
/***/ (function(module, __webpack_exports__, __webpack_require__) {

"use strict";
__webpack_require__.r(__webpack_exports__);
/* harmony export (binding) */ __webpack_require__.d(__webpack_exports__, "BsCalendarLayoutComponent", function() { return BsCalendarLayoutComponent; });
/* harmony import */ var _angular_core__WEBPACK_IMPORTED_MODULE_0__ = __webpack_require__(/*! @angular/core */ "./node_modules/@angular/core/fesm5/core.js");

var BsCalendarLayoutComponent = /** @class */ (function () {
    function BsCalendarLayoutComponent() {
    }
    BsCalendarLayoutComponent.decorators = [
        { type: _angular_core__WEBPACK_IMPORTED_MODULE_0__["Component"], args: [{
                    selector: 'bs-calendar-layout',
                    template: "\n    <!-- current date, will be added in nearest releases -->\n    <bs-current-date title=\"hey there\" *ngIf=\"false\"></bs-current-date>\n\n    <!--navigation-->\n    <div class=\"bs-datepicker-head\">\n      <ng-content select=\"bs-datepicker-navigation-view\"></ng-content>\n    </div>\n\n    <div class=\"bs-datepicker-body\">\n      <ng-content></ng-content>\n    </div>\n\n    <!--timepicker-->\n    <bs-timepicker *ngIf=\"false\"></bs-timepicker>\n  "
                },] },
    ];
    return BsCalendarLayoutComponent;
}());

//# sourceMappingURL=bs-calendar-layout.component.js.map

/***/ }),

/***/ "./node_modules/ngx-bootstrap/datepicker/themes/bs/bs-current-date-view.component.js":
/*!*******************************************************************************************!*\
  !*** ./node_modules/ngx-bootstrap/datepicker/themes/bs/bs-current-date-view.component.js ***!
  \*******************************************************************************************/
/*! exports provided: BsCurrentDateViewComponent */
/***/ (function(module, __webpack_exports__, __webpack_require__) {

"use strict";
__webpack_require__.r(__webpack_exports__);
/* harmony export (binding) */ __webpack_require__.d(__webpack_exports__, "BsCurrentDateViewComponent", function() { return BsCurrentDateViewComponent; });
/* harmony import */ var _angular_core__WEBPACK_IMPORTED_MODULE_0__ = __webpack_require__(/*! @angular/core */ "./node_modules/@angular/core/fesm5/core.js");

var BsCurrentDateViewComponent = /** @class */ (function () {
    function BsCurrentDateViewComponent() {
    }
    BsCurrentDateViewComponent.decorators = [
        { type: _angular_core__WEBPACK_IMPORTED_MODULE_0__["Component"], args: [{
                    selector: 'bs-current-date',
                    template: "<div class=\"current-timedate\"><span>{{ title }}</span></div>"
                },] },
    ];
    /** @nocollapse */
    BsCurrentDateViewComponent.propDecorators = {
        "title": [{ type: _angular_core__WEBPACK_IMPORTED_MODULE_0__["Input"] },],
    };
    return BsCurrentDateViewComponent;
}());

//# sourceMappingURL=bs-current-date-view.component.js.map

/***/ }),

/***/ "./node_modules/ngx-bootstrap/datepicker/themes/bs/bs-custom-dates-view.component.js":
/*!*******************************************************************************************!*\
  !*** ./node_modules/ngx-bootstrap/datepicker/themes/bs/bs-custom-dates-view.component.js ***!
  \*******************************************************************************************/
/*! exports provided: BsCustomDatesViewComponent */
/***/ (function(module, __webpack_exports__, __webpack_require__) {

"use strict";
__webpack_require__.r(__webpack_exports__);
/* harmony export (binding) */ __webpack_require__.d(__webpack_exports__, "BsCustomDatesViewComponent", function() { return BsCustomDatesViewComponent; });
/* harmony import */ var _angular_core__WEBPACK_IMPORTED_MODULE_0__ = __webpack_require__(/*! @angular/core */ "./node_modules/@angular/core/fesm5/core.js");

var BsCustomDatesViewComponent = /** @class */ (function () {
    function BsCustomDatesViewComponent() {
    }
    BsCustomDatesViewComponent.decorators = [
        { type: _angular_core__WEBPACK_IMPORTED_MODULE_0__["Component"], args: [{
                    selector: 'bs-custom-date-view',
                    template: "\n    <div class=\"bs-datepicker-predefined-btns\">\n      <button *ngFor=\"let range of ranges\">{{ range.label }}</button>\n      <button *ngIf=\"isCustomRangeShown\">Custom Range</button>\n    </div>\n  ",
                    changeDetection: _angular_core__WEBPACK_IMPORTED_MODULE_0__["ChangeDetectionStrategy"].OnPush
                },] },
    ];
    /** @nocollapse */
    BsCustomDatesViewComponent.propDecorators = {
        "isCustomRangeShown": [{ type: _angular_core__WEBPACK_IMPORTED_MODULE_0__["Input"] },],
        "ranges": [{ type: _angular_core__WEBPACK_IMPORTED_MODULE_0__["Input"] },],
    };
    return BsCustomDatesViewComponent;
}());

//# sourceMappingURL=bs-custom-dates-view.component.js.map

/***/ }),

/***/ "./node_modules/ngx-bootstrap/datepicker/themes/bs/bs-datepicker-container.component.js":
/*!**********************************************************************************************!*\
  !*** ./node_modules/ngx-bootstrap/datepicker/themes/bs/bs-datepicker-container.component.js ***!
  \**********************************************************************************************/
/*! exports provided: BsDatepickerContainerComponent */
/***/ (function(module, __webpack_exports__, __webpack_require__) {

"use strict";
__webpack_require__.r(__webpack_exports__);
/* harmony export (binding) */ __webpack_require__.d(__webpack_exports__, "BsDatepickerContainerComponent", function() { return BsDatepickerContainerComponent; });
/* harmony import */ var _angular_core__WEBPACK_IMPORTED_MODULE_0__ = __webpack_require__(/*! @angular/core */ "./node_modules/@angular/core/fesm5/core.js");
/* harmony import */ var _base_bs_datepicker_container__WEBPACK_IMPORTED_MODULE_1__ = __webpack_require__(/*! ../../base/bs-datepicker-container */ "./node_modules/ngx-bootstrap/datepicker/base/bs-datepicker-container.js");
/* harmony import */ var _bs_datepicker_config__WEBPACK_IMPORTED_MODULE_2__ = __webpack_require__(/*! ../../bs-datepicker.config */ "./node_modules/ngx-bootstrap/datepicker/bs-datepicker.config.js");
/* harmony import */ var _reducer_bs_datepicker_actions__WEBPACK_IMPORTED_MODULE_3__ = __webpack_require__(/*! ../../reducer/bs-datepicker.actions */ "./node_modules/ngx-bootstrap/datepicker/reducer/bs-datepicker.actions.js");
/* harmony import */ var _reducer_bs_datepicker_effects__WEBPACK_IMPORTED_MODULE_4__ = __webpack_require__(/*! ../../reducer/bs-datepicker.effects */ "./node_modules/ngx-bootstrap/datepicker/reducer/bs-datepicker.effects.js");
/* harmony import */ var _reducer_bs_datepicker_store__WEBPACK_IMPORTED_MODULE_5__ = __webpack_require__(/*! ../../reducer/bs-datepicker.store */ "./node_modules/ngx-bootstrap/datepicker/reducer/bs-datepicker.store.js");
var __extends = (undefined && undefined.__extends) || (function () {
    var extendStatics = Object.setPrototypeOf ||
        ({ __proto__: [] } instanceof Array && function (d, b) { d.__proto__ = b; }) ||
        function (d, b) { for (var p in b) if (b.hasOwnProperty(p)) d[p] = b[p]; };
    return function (d, b) {
        extendStatics(d, b);
        function __() { this.constructor = d; }
        d.prototype = b === null ? Object.create(b) : (__.prototype = b.prototype, new __());
    };
})();






var BsDatepickerContainerComponent = /** @class */ (function (_super) {
    __extends(BsDatepickerContainerComponent, _super);
    function BsDatepickerContainerComponent(_config, _store, _actions, _effects) {
        var _this = _super.call(this) || this;
        _this._config = _config;
        _this._store = _store;
        _this._actions = _actions;
        _this.valueChange = new _angular_core__WEBPACK_IMPORTED_MODULE_0__["EventEmitter"]();
        _this._subs = [];
        _this._effects = _effects;
        return _this;
    }
    Object.defineProperty(BsDatepickerContainerComponent.prototype, "value", {
        set: function (value) {
            this._effects.setValue(value);
        },
        enumerable: true,
        configurable: true
    });
    BsDatepickerContainerComponent.prototype.ngOnInit = function () {
        var _this = this;
        this.containerClass = this._config.containerClass;
        this._effects
            .init(this._store)
            .setOptions(this._config)
            .setBindings(this)
            .setEventHandlers(this)
            .registerDatepickerSideEffects();
        // todo: move it somewhere else
        // on selected date change
        this._subs.push(this._store
            .select(function (state) { return state.selectedDate; })
            .subscribe(function (date) { return _this.valueChange.emit(date); }));
    };
    BsDatepickerContainerComponent.prototype.daySelectHandler = function (day) {
        if (day.isOtherMonth || day.isDisabled) {
            return;
        }
        this._store.dispatch(this._actions.select(day.date));
    };
    BsDatepickerContainerComponent.prototype.ngOnDestroy = function () {
        for (var _i = 0, _a = this._subs; _i < _a.length; _i++) {
            var sub = _a[_i];
            sub.unsubscribe();
        }
        this._effects.destroy();
    };
    BsDatepickerContainerComponent.decorators = [
        { type: _angular_core__WEBPACK_IMPORTED_MODULE_0__["Component"], args: [{
                    selector: 'bs-datepicker-container',
                    providers: [_reducer_bs_datepicker_store__WEBPACK_IMPORTED_MODULE_5__["BsDatepickerStore"], _reducer_bs_datepicker_effects__WEBPACK_IMPORTED_MODULE_4__["BsDatepickerEffects"]],
                    template: "<!-- days calendar view mode --> <div class=\"bs-datepicker\" [ngClass]=\"containerClass\" *ngIf=\"viewMode | async\"> <div class=\"bs-datepicker-container\"> <!--calendars--> <div class=\"bs-calendar-container\" [ngSwitch]=\"viewMode | async\" role=\"application\"> <!--days calendar--> <div *ngSwitchCase=\"'day'\"> <bs-days-calendar-view *ngFor=\"let calendar of (daysCalendar | async)\" [class.bs-datepicker-multiple]=\"(daysCalendar | async)?.length > 1\" [calendar]=\"calendar\" [options]=\"options | async\" (onNavigate)=\"navigateTo($event)\" (onViewMode)=\"setViewMode($event)\" (onHover)=\"dayHoverHandler($event)\" (onSelect)=\"daySelectHandler($event)\" ></bs-days-calendar-view> </div> <!--months calendar--> <div *ngSwitchCase=\"'month'\"> <bs-month-calendar-view *ngFor=\"let calendar of (monthsCalendar | async)\" [class.bs-datepicker-multiple]=\"(daysCalendar | async)?.length > 1\" [calendar]=\"calendar\" (onNavigate)=\"navigateTo($event)\" (onViewMode)=\"setViewMode($event)\" (onHover)=\"monthHoverHandler($event)\" (onSelect)=\"monthSelectHandler($event)\" ></bs-month-calendar-view> </div> <!--years calendar--> <div *ngSwitchCase=\"'year'\"> <bs-years-calendar-view *ngFor=\"let calendar of (yearsCalendar | async)\" [class.bs-datepicker-multiple]=\"(daysCalendar | async)?.length > 1\" [calendar]=\"calendar\" (onNavigate)=\"navigateTo($event)\" (onViewMode)=\"setViewMode($event)\" (onHover)=\"yearHoverHandler($event)\" (onSelect)=\"yearSelectHandler($event)\" ></bs-years-calendar-view> </div> </div> <!--applycancel buttons--> <div class=\"bs-datepicker-buttons\" *ngIf=\"false\"> <button class=\"btn btn-success\">Apply</button> <button class=\"btn btn-default\">Cancel</button> </div> </div> <!--custom dates or date ranges picker--> <div class=\"bs-datepicker-custom-range\" *ngIf=\"false\"> <bs-custom-date-view [ranges]=\"_customRangesFish\"></bs-custom-date-view> </div> </div> ",
                    host: {
                        '(click)': '_stopPropagation($event)',
                        style: 'position: absolute; display: block;',
                        role: 'dialog',
                        'aria-label': 'calendar'
                    }
                },] },
    ];
    /** @nocollapse */
    BsDatepickerContainerComponent.ctorParameters = function () { return [
        { type: _bs_datepicker_config__WEBPACK_IMPORTED_MODULE_2__["BsDatepickerConfig"], },
        { type: _reducer_bs_datepicker_store__WEBPACK_IMPORTED_MODULE_5__["BsDatepickerStore"], },
        { type: _reducer_bs_datepicker_actions__WEBPACK_IMPORTED_MODULE_3__["BsDatepickerActions"], },
        { type: _reducer_bs_datepicker_effects__WEBPACK_IMPORTED_MODULE_4__["BsDatepickerEffects"], },
    ]; };
    return BsDatepickerContainerComponent;
}(_base_bs_datepicker_container__WEBPACK_IMPORTED_MODULE_1__["BsDatepickerAbstractComponent"]));

//# sourceMappingURL=bs-datepicker-container.component.js.map

/***/ }),

/***/ "./node_modules/ngx-bootstrap/datepicker/themes/bs/bs-datepicker-day-decorator.directive.js":
/*!**************************************************************************************************!*\
  !*** ./node_modules/ngx-bootstrap/datepicker/themes/bs/bs-datepicker-day-decorator.directive.js ***!
  \**************************************************************************************************/
/*! exports provided: BsDatepickerDayDecoratorComponent */
/***/ (function(module, __webpack_exports__, __webpack_require__) {

"use strict";
__webpack_require__.r(__webpack_exports__);
/* harmony export (binding) */ __webpack_require__.d(__webpack_exports__, "BsDatepickerDayDecoratorComponent", function() { return BsDatepickerDayDecoratorComponent; });
/* harmony import */ var _angular_core__WEBPACK_IMPORTED_MODULE_0__ = __webpack_require__(/*! @angular/core */ "./node_modules/@angular/core/fesm5/core.js");

var BsDatepickerDayDecoratorComponent = /** @class */ (function () {
    function BsDatepickerDayDecoratorComponent() {
    }
    BsDatepickerDayDecoratorComponent.decorators = [
        { type: _angular_core__WEBPACK_IMPORTED_MODULE_0__["Component"], args: [{
                    selector: '[bsDatepickerDayDecorator]',
                    changeDetection: _angular_core__WEBPACK_IMPORTED_MODULE_0__["ChangeDetectionStrategy"].OnPush,
                    host: {
                        '[class.disabled]': 'day.isDisabled',
                        '[class.is-highlighted]': 'day.isHovered',
                        '[class.is-other-month]': 'day.isOtherMonth',
                        '[class.in-range]': 'day.isInRange',
                        '[class.select-start]': 'day.isSelectionStart',
                        '[class.select-end]': 'day.isSelectionEnd',
                        '[class.selected]': 'day.isSelected'
                    },
                    template: "{{ day.label }}"
                },] },
    ];
    /** @nocollapse */
    BsDatepickerDayDecoratorComponent.propDecorators = {
        "day": [{ type: _angular_core__WEBPACK_IMPORTED_MODULE_0__["Input"] },],
    };
    return BsDatepickerDayDecoratorComponent;
}());

//# sourceMappingURL=bs-datepicker-day-decorator.directive.js.map

/***/ }),

/***/ "./node_modules/ngx-bootstrap/datepicker/themes/bs/bs-datepicker-navigation-view.component.js":
/*!****************************************************************************************************!*\
  !*** ./node_modules/ngx-bootstrap/datepicker/themes/bs/bs-datepicker-navigation-view.component.js ***!
  \****************************************************************************************************/
/*! exports provided: BsDatepickerNavigationViewComponent */
/***/ (function(module, __webpack_exports__, __webpack_require__) {

"use strict";
__webpack_require__.r(__webpack_exports__);
/* harmony export (binding) */ __webpack_require__.d(__webpack_exports__, "BsDatepickerNavigationViewComponent", function() { return BsDatepickerNavigationViewComponent; });
/* harmony import */ var _angular_core__WEBPACK_IMPORTED_MODULE_0__ = __webpack_require__(/*! @angular/core */ "./node_modules/@angular/core/fesm5/core.js");
/* harmony import */ var _models_index__WEBPACK_IMPORTED_MODULE_1__ = __webpack_require__(/*! ../../models/index */ "./node_modules/ngx-bootstrap/datepicker/models/index.js");


var BsDatepickerNavigationViewComponent = /** @class */ (function () {
    function BsDatepickerNavigationViewComponent() {
        this.onNavigate = new _angular_core__WEBPACK_IMPORTED_MODULE_0__["EventEmitter"]();
        this.onViewMode = new _angular_core__WEBPACK_IMPORTED_MODULE_0__["EventEmitter"]();
    }
    BsDatepickerNavigationViewComponent.prototype.navTo = function (down) {
        this.onNavigate.emit(down ? _models_index__WEBPACK_IMPORTED_MODULE_1__["BsNavigationDirection"].DOWN : _models_index__WEBPACK_IMPORTED_MODULE_1__["BsNavigationDirection"].UP);
    };
    BsDatepickerNavigationViewComponent.prototype.view = function (viewMode) {
        this.onViewMode.emit(viewMode);
    };
    BsDatepickerNavigationViewComponent.decorators = [
        { type: _angular_core__WEBPACK_IMPORTED_MODULE_0__["Component"], args: [{
                    selector: 'bs-datepicker-navigation-view',
                    changeDetection: _angular_core__WEBPACK_IMPORTED_MODULE_0__["ChangeDetectionStrategy"].OnPush,
                    template: "\n    <button class=\"previous\"\n            [disabled]=\"calendar.disableLeftArrow\"\n            [style.visibility]=\"calendar.hideLeftArrow ? 'hidden' : 'visible'\"\n            (click)=\"navTo(true)\"><span>&lsaquo;</span>\n    </button>\n\n    <button class=\"current\"\n            *ngIf=\"calendar.monthTitle\"\n            (click)=\"view('month')\"\n    ><span>{{ calendar.monthTitle }}</span>\n    </button>\n\n    <button class=\"current\" (click)=\"view('year')\"\n    ><span>{{ calendar.yearTitle }}</span></button>\n\n    <button class=\"next\"\n            [disabled]=\"calendar.disableRightArrow\"\n            [style.visibility]=\"calendar.hideRightArrow ? 'hidden' : 'visible'\"\n            (click)=\"navTo(false)\"><span>&rsaquo;</span>\n    </button>\n  "
                },] },
    ];
    /** @nocollapse */
    BsDatepickerNavigationViewComponent.propDecorators = {
        "calendar": [{ type: _angular_core__WEBPACK_IMPORTED_MODULE_0__["Input"] },],
        "onNavigate": [{ type: _angular_core__WEBPACK_IMPORTED_MODULE_0__["Output"] },],
        "onViewMode": [{ type: _angular_core__WEBPACK_IMPORTED_MODULE_0__["Output"] },],
    };
    return BsDatepickerNavigationViewComponent;
}());

//# sourceMappingURL=bs-datepicker-navigation-view.component.js.map

/***/ }),

/***/ "./node_modules/ngx-bootstrap/datepicker/themes/bs/bs-daterangepicker-container.component.js":
/*!***************************************************************************************************!*\
  !*** ./node_modules/ngx-bootstrap/datepicker/themes/bs/bs-daterangepicker-container.component.js ***!
  \***************************************************************************************************/
/*! exports provided: BsDaterangepickerContainerComponent */
/***/ (function(module, __webpack_exports__, __webpack_require__) {

"use strict";
__webpack_require__.r(__webpack_exports__);
/* harmony export (binding) */ __webpack_require__.d(__webpack_exports__, "BsDaterangepickerContainerComponent", function() { return BsDaterangepickerContainerComponent; });
/* harmony import */ var _angular_core__WEBPACK_IMPORTED_MODULE_0__ = __webpack_require__(/*! @angular/core */ "./node_modules/@angular/core/fesm5/core.js");
/* harmony import */ var _base_bs_datepicker_container__WEBPACK_IMPORTED_MODULE_1__ = __webpack_require__(/*! ../../base/bs-datepicker-container */ "./node_modules/ngx-bootstrap/datepicker/base/bs-datepicker-container.js");
/* harmony import */ var _bs_datepicker_config__WEBPACK_IMPORTED_MODULE_2__ = __webpack_require__(/*! ../../bs-datepicker.config */ "./node_modules/ngx-bootstrap/datepicker/bs-datepicker.config.js");
/* harmony import */ var _reducer_bs_datepicker_actions__WEBPACK_IMPORTED_MODULE_3__ = __webpack_require__(/*! ../../reducer/bs-datepicker.actions */ "./node_modules/ngx-bootstrap/datepicker/reducer/bs-datepicker.actions.js");
/* harmony import */ var _reducer_bs_datepicker_effects__WEBPACK_IMPORTED_MODULE_4__ = __webpack_require__(/*! ../../reducer/bs-datepicker.effects */ "./node_modules/ngx-bootstrap/datepicker/reducer/bs-datepicker.effects.js");
/* harmony import */ var _reducer_bs_datepicker_store__WEBPACK_IMPORTED_MODULE_5__ = __webpack_require__(/*! ../../reducer/bs-datepicker.store */ "./node_modules/ngx-bootstrap/datepicker/reducer/bs-datepicker.store.js");
var __extends = (undefined && undefined.__extends) || (function () {
    var extendStatics = Object.setPrototypeOf ||
        ({ __proto__: [] } instanceof Array && function (d, b) { d.__proto__ = b; }) ||
        function (d, b) { for (var p in b) if (b.hasOwnProperty(p)) d[p] = b[p]; };
    return function (d, b) {
        extendStatics(d, b);
        function __() { this.constructor = d; }
        d.prototype = b === null ? Object.create(b) : (__.prototype = b.prototype, new __());
    };
})();






var BsDaterangepickerContainerComponent = /** @class */ (function (_super) {
    __extends(BsDaterangepickerContainerComponent, _super);
    function BsDaterangepickerContainerComponent(_config, _store, _actions, _effects) {
        var _this = _super.call(this) || this;
        _this._config = _config;
        _this._store = _store;
        _this._actions = _actions;
        _this.valueChange = new _angular_core__WEBPACK_IMPORTED_MODULE_0__["EventEmitter"]();
        _this._rangeStack = [];
        _this._subs = [];
        _this._effects = _effects;
        return _this;
    }
    Object.defineProperty(BsDaterangepickerContainerComponent.prototype, "value", {
        set: function (value) {
            this._effects.setRangeValue(value);
        },
        enumerable: true,
        configurable: true
    });
    BsDaterangepickerContainerComponent.prototype.ngOnInit = function () {
        var _this = this;
        this.containerClass = this._config.containerClass;
        this._effects
            .init(this._store)
            .setOptions(this._config)
            .setBindings(this)
            .setEventHandlers(this)
            .registerDatepickerSideEffects();
        // todo: move it somewhere else
        // on selected date change
        this._subs.push(this._store
            .select(function (state) { return state.selectedRange; })
            .subscribe(function (date) { return _this.valueChange.emit(date); }));
    };
    BsDaterangepickerContainerComponent.prototype.daySelectHandler = function (day) {
        if (day.isOtherMonth || day.isDisabled) {
            return;
        }
        // if only one date is already selected
        // and user clicks on previous date
        // start selection from new date
        // but if new date is after initial one
        // than finish selection
        if (this._rangeStack.length === 1) {
            this._rangeStack =
                day.date >= this._rangeStack[0]
                    ? [this._rangeStack[0], day.date]
                    : [day.date];
        }
        if (this._rangeStack.length === 0) {
            this._rangeStack = [day.date];
        }
        this._store.dispatch(this._actions.selectRange(this._rangeStack));
        if (this._rangeStack.length === 2) {
            this._rangeStack = [];
        }
    };
    BsDaterangepickerContainerComponent.prototype.ngOnDestroy = function () {
        for (var _i = 0, _a = this._subs; _i < _a.length; _i++) {
            var sub = _a[_i];
            sub.unsubscribe();
        }
        this._effects.destroy();
    };
    BsDaterangepickerContainerComponent.decorators = [
        { type: _angular_core__WEBPACK_IMPORTED_MODULE_0__["Component"], args: [{
                    selector: 'bs-daterangepicker-container',
                    providers: [_reducer_bs_datepicker_store__WEBPACK_IMPORTED_MODULE_5__["BsDatepickerStore"], _reducer_bs_datepicker_effects__WEBPACK_IMPORTED_MODULE_4__["BsDatepickerEffects"]],
                    template: "<!-- days calendar view mode --> <div class=\"bs-datepicker\" [ngClass]=\"containerClass\" *ngIf=\"viewMode | async\"> <div class=\"bs-datepicker-container\"> <!--calendars--> <div class=\"bs-calendar-container\" [ngSwitch]=\"viewMode | async\" role=\"application\"> <!--days calendar--> <div *ngSwitchCase=\"'day'\"> <bs-days-calendar-view *ngFor=\"let calendar of (daysCalendar | async)\" [class.bs-datepicker-multiple]=\"(daysCalendar | async)?.length > 1\" [calendar]=\"calendar\" [options]=\"options | async\" (onNavigate)=\"navigateTo($event)\" (onViewMode)=\"setViewMode($event)\" (onHover)=\"dayHoverHandler($event)\" (onSelect)=\"daySelectHandler($event)\" ></bs-days-calendar-view> </div> <!--months calendar--> <div *ngSwitchCase=\"'month'\"> <bs-month-calendar-view *ngFor=\"let calendar of (monthsCalendar | async)\" [class.bs-datepicker-multiple]=\"(daysCalendar | async)?.length > 1\" [calendar]=\"calendar\" (onNavigate)=\"navigateTo($event)\" (onViewMode)=\"setViewMode($event)\" (onHover)=\"monthHoverHandler($event)\" (onSelect)=\"monthSelectHandler($event)\" ></bs-month-calendar-view> </div> <!--years calendar--> <div *ngSwitchCase=\"'year'\"> <bs-years-calendar-view *ngFor=\"let calendar of (yearsCalendar | async)\" [class.bs-datepicker-multiple]=\"(daysCalendar | async)?.length > 1\" [calendar]=\"calendar\" (onNavigate)=\"navigateTo($event)\" (onViewMode)=\"setViewMode($event)\" (onHover)=\"yearHoverHandler($event)\" (onSelect)=\"yearSelectHandler($event)\" ></bs-years-calendar-view> </div> </div> <!--applycancel buttons--> <div class=\"bs-datepicker-buttons\" *ngIf=\"false\"> <button class=\"btn btn-success\">Apply</button> <button class=\"btn btn-default\">Cancel</button> </div> </div> <!--custom dates or date ranges picker--> <div class=\"bs-datepicker-custom-range\" *ngIf=\"false\"> <bs-custom-date-view [ranges]=\"_customRangesFish\"></bs-custom-date-view> </div> </div> ",
                    host: {
                        '(click)': '_stopPropagation($event)',
                        style: 'position: absolute; display: block;',
                        role: 'dialog',
                        'aria-label': 'calendar'
                    }
                },] },
    ];
    /** @nocollapse */
    BsDaterangepickerContainerComponent.ctorParameters = function () { return [
        { type: _bs_datepicker_config__WEBPACK_IMPORTED_MODULE_2__["BsDatepickerConfig"], },
        { type: _reducer_bs_datepicker_store__WEBPACK_IMPORTED_MODULE_5__["BsDatepickerStore"], },
        { type: _reducer_bs_datepicker_actions__WEBPACK_IMPORTED_MODULE_3__["BsDatepickerActions"], },
        { type: _reducer_bs_datepicker_effects__WEBPACK_IMPORTED_MODULE_4__["BsDatepickerEffects"], },
    ]; };
    return BsDaterangepickerContainerComponent;
}(_base_bs_datepicker_container__WEBPACK_IMPORTED_MODULE_1__["BsDatepickerAbstractComponent"]));

//# sourceMappingURL=bs-daterangepicker-container.component.js.map

/***/ }),

/***/ "./node_modules/ngx-bootstrap/datepicker/themes/bs/bs-days-calendar-view.component.js":
/*!********************************************************************************************!*\
  !*** ./node_modules/ngx-bootstrap/datepicker/themes/bs/bs-days-calendar-view.component.js ***!
  \********************************************************************************************/
/*! exports provided: BsDaysCalendarViewComponent */
/***/ (function(module, __webpack_exports__, __webpack_require__) {

"use strict";
__webpack_require__.r(__webpack_exports__);
/* harmony export (binding) */ __webpack_require__.d(__webpack_exports__, "BsDaysCalendarViewComponent", function() { return BsDaysCalendarViewComponent; });
/* harmony import */ var _angular_core__WEBPACK_IMPORTED_MODULE_0__ = __webpack_require__(/*! @angular/core */ "./node_modules/@angular/core/fesm5/core.js");
/* harmony import */ var _models_index__WEBPACK_IMPORTED_MODULE_1__ = __webpack_require__(/*! ../../models/index */ "./node_modules/ngx-bootstrap/datepicker/models/index.js");


var BsDaysCalendarViewComponent = /** @class */ (function () {
    function BsDaysCalendarViewComponent() {
        this.onNavigate = new _angular_core__WEBPACK_IMPORTED_MODULE_0__["EventEmitter"]();
        this.onViewMode = new _angular_core__WEBPACK_IMPORTED_MODULE_0__["EventEmitter"]();
        this.onSelect = new _angular_core__WEBPACK_IMPORTED_MODULE_0__["EventEmitter"]();
        this.onHover = new _angular_core__WEBPACK_IMPORTED_MODULE_0__["EventEmitter"]();
    }
    BsDaysCalendarViewComponent.prototype.navigateTo = function (event) {
        var step = _models_index__WEBPACK_IMPORTED_MODULE_1__["BsNavigationDirection"].DOWN === event ? -1 : 1;
        this.onNavigate.emit({ step: { month: step } });
    };
    BsDaysCalendarViewComponent.prototype.changeViewMode = function (event) {
        this.onViewMode.emit(event);
    };
    BsDaysCalendarViewComponent.prototype.selectDay = function (event) {
        this.onSelect.emit(event);
    };
    BsDaysCalendarViewComponent.prototype.hoverDay = function (cell, isHovered) {
        this.onHover.emit({ cell: cell, isHovered: isHovered });
    };
    BsDaysCalendarViewComponent.decorators = [
        { type: _angular_core__WEBPACK_IMPORTED_MODULE_0__["Component"], args: [{
                    selector: 'bs-days-calendar-view',
                    // changeDetection: ChangeDetectionStrategy.OnPush,
                    template: "\n    <bs-calendar-layout>\n      <bs-datepicker-navigation-view\n        [calendar]=\"calendar\"\n        (onNavigate)=\"navigateTo($event)\"\n        (onViewMode)=\"changeViewMode($event)\"\n      ></bs-datepicker-navigation-view>\n\n      <!--days matrix-->\n      <table role=\"grid\" class=\"days weeks\">\n        <thead>\n        <tr>\n          <!--if show weeks-->\n          <th *ngIf=\"options.showWeekNumbers\"></th>\n          <th *ngFor=\"let weekday of calendar.weekdays; let i = index\"\n              aria-label=\"weekday\">{{ calendar.weekdays[i] }}\n          </th>\n        </tr>\n        </thead>\n        <tbody>\n        <tr *ngFor=\"let week of calendar.weeks; let i = index\">\n          <td class=\"week\" *ngIf=\"options.showWeekNumbers\">\n            <span>{{ calendar.weekNumbers[i] }}</span>\n          </td>\n          <td *ngFor=\"let day of week.days\" role=\"gridcell\">\n          <span bsDatepickerDayDecorator\n                [day]=\"day\"\n                (click)=\"selectDay(day)\"\n                (mouseenter)=\"hoverDay(day, true)\"\n                (mouseleave)=\"hoverDay(day, false)\">{{ day.label }}</span>\n          </td>\n        </tr>\n        </tbody>\n      </table>\n\n    </bs-calendar-layout>\n  "
                },] },
    ];
    /** @nocollapse */
    BsDaysCalendarViewComponent.propDecorators = {
        "calendar": [{ type: _angular_core__WEBPACK_IMPORTED_MODULE_0__["Input"] },],
        "options": [{ type: _angular_core__WEBPACK_IMPORTED_MODULE_0__["Input"] },],
        "onNavigate": [{ type: _angular_core__WEBPACK_IMPORTED_MODULE_0__["Output"] },],
        "onViewMode": [{ type: _angular_core__WEBPACK_IMPORTED_MODULE_0__["Output"] },],
        "onSelect": [{ type: _angular_core__WEBPACK_IMPORTED_MODULE_0__["Output"] },],
        "onHover": [{ type: _angular_core__WEBPACK_IMPORTED_MODULE_0__["Output"] },],
    };
    return BsDaysCalendarViewComponent;
}());

//# sourceMappingURL=bs-days-calendar-view.component.js.map

/***/ }),

/***/ "./node_modules/ngx-bootstrap/datepicker/themes/bs/bs-months-calendar-view.component.js":
/*!**********************************************************************************************!*\
  !*** ./node_modules/ngx-bootstrap/datepicker/themes/bs/bs-months-calendar-view.component.js ***!
  \**********************************************************************************************/
/*! exports provided: BsMonthCalendarViewComponent */
/***/ (function(module, __webpack_exports__, __webpack_require__) {

"use strict";
__webpack_require__.r(__webpack_exports__);
/* harmony export (binding) */ __webpack_require__.d(__webpack_exports__, "BsMonthCalendarViewComponent", function() { return BsMonthCalendarViewComponent; });
/* harmony import */ var _angular_core__WEBPACK_IMPORTED_MODULE_0__ = __webpack_require__(/*! @angular/core */ "./node_modules/@angular/core/fesm5/core.js");
/* harmony import */ var _models_index__WEBPACK_IMPORTED_MODULE_1__ = __webpack_require__(/*! ../../models/index */ "./node_modules/ngx-bootstrap/datepicker/models/index.js");


var BsMonthCalendarViewComponent = /** @class */ (function () {
    function BsMonthCalendarViewComponent() {
        this.onNavigate = new _angular_core__WEBPACK_IMPORTED_MODULE_0__["EventEmitter"]();
        this.onViewMode = new _angular_core__WEBPACK_IMPORTED_MODULE_0__["EventEmitter"]();
        this.onSelect = new _angular_core__WEBPACK_IMPORTED_MODULE_0__["EventEmitter"]();
        this.onHover = new _angular_core__WEBPACK_IMPORTED_MODULE_0__["EventEmitter"]();
    }
    BsMonthCalendarViewComponent.prototype.navigateTo = function (event) {
        var step = _models_index__WEBPACK_IMPORTED_MODULE_1__["BsNavigationDirection"].DOWN === event ? -1 : 1;
        this.onNavigate.emit({ step: { year: step } });
    };
    BsMonthCalendarViewComponent.prototype.viewMonth = function (month) {
        this.onSelect.emit(month);
    };
    BsMonthCalendarViewComponent.prototype.hoverMonth = function (cell, isHovered) {
        this.onHover.emit({ cell: cell, isHovered: isHovered });
    };
    BsMonthCalendarViewComponent.prototype.changeViewMode = function (event) {
        this.onViewMode.emit(event);
    };
    BsMonthCalendarViewComponent.decorators = [
        { type: _angular_core__WEBPACK_IMPORTED_MODULE_0__["Component"], args: [{
                    selector: 'bs-month-calendar-view',
                    template: "\n    <bs-calendar-layout>\n      <bs-datepicker-navigation-view\n        [calendar]=\"calendar\"\n        (onNavigate)=\"navigateTo($event)\"\n        (onViewMode)=\"changeViewMode($event)\"\n      ></bs-datepicker-navigation-view>\n\n      <table role=\"grid\" class=\"months\">\n        <tbody>\n        <tr *ngFor=\"let row of calendar.months\">\n          <td *ngFor=\"let month of row\" role=\"gridcell\"\n              (click)=\"viewMonth(month)\"\n              (mouseenter)=\"hoverMonth(month, true)\"\n              (mouseleave)=\"hoverMonth(month, false)\"\n              [class.disabled]=\"month.isDisabled\"\n              [class.is-highlighted]=\"month.isHovered\">\n            <span>{{ month.label }}</span>\n          </td>\n        </tr>\n        </tbody>\n      </table>\n    </bs-calendar-layout>\n  "
                },] },
    ];
    /** @nocollapse */
    BsMonthCalendarViewComponent.propDecorators = {
        "calendar": [{ type: _angular_core__WEBPACK_IMPORTED_MODULE_0__["Input"] },],
        "onNavigate": [{ type: _angular_core__WEBPACK_IMPORTED_MODULE_0__["Output"] },],
        "onViewMode": [{ type: _angular_core__WEBPACK_IMPORTED_MODULE_0__["Output"] },],
        "onSelect": [{ type: _angular_core__WEBPACK_IMPORTED_MODULE_0__["Output"] },],
        "onHover": [{ type: _angular_core__WEBPACK_IMPORTED_MODULE_0__["Output"] },],
    };
    return BsMonthCalendarViewComponent;
}());

//# sourceMappingURL=bs-months-calendar-view.component.js.map

/***/ }),

/***/ "./node_modules/ngx-bootstrap/datepicker/themes/bs/bs-timepicker-view.component.js":
/*!*****************************************************************************************!*\
  !*** ./node_modules/ngx-bootstrap/datepicker/themes/bs/bs-timepicker-view.component.js ***!
  \*****************************************************************************************/
/*! exports provided: BsTimepickerViewComponent */
/***/ (function(module, __webpack_exports__, __webpack_require__) {

"use strict";
__webpack_require__.r(__webpack_exports__);
/* harmony export (binding) */ __webpack_require__.d(__webpack_exports__, "BsTimepickerViewComponent", function() { return BsTimepickerViewComponent; });
/* harmony import */ var _angular_core__WEBPACK_IMPORTED_MODULE_0__ = __webpack_require__(/*! @angular/core */ "./node_modules/@angular/core/fesm5/core.js");

var BsTimepickerViewComponent = /** @class */ (function () {
    function BsTimepickerViewComponent() {
        this.ampm = 'ok';
        this.hours = 0;
        this.minutes = 0;
    }
    BsTimepickerViewComponent.decorators = [
        { type: _angular_core__WEBPACK_IMPORTED_MODULE_0__["Component"], args: [{
                    selector: 'bs-timepicker',
                    template: "\n    <div class=\"bs-timepicker-container\">\n      <div class=\"bs-timepicker-controls\">\n        <button class=\"bs-decrease\">-</button>\n        <input type=\"text\" [value]=\"hours\" placeholder=\"00\">\n        <button class=\"bs-increase\">+</button>\n      </div>\n      <div class=\"bs-timepicker-controls\">\n        <button class=\"bs-decrease\">-</button>\n        <input type=\"text\" [value]=\"minutes\" placeholder=\"00\">\n        <button class=\"bs-increase\">+</button>\n      </div>\n      <button class=\"switch-time-format\">{{ ampm }}\n        <img\n          src=\"data:image/png;base64,iVBORw0KGgoAAAANSUhEUgAAAAsAAAAKCAYAAABi8KSDAAABSElEQVQYV3XQPUvDUBQG4HNuagtVqc6KgouCv6GIuIntYBLB9hcIQpLStCAIV7DYmpTcRWcXqZio3Vwc/UCc/QEqfgyKGbr0I7nS1EiHeqYzPO/h5SD0jaxUZjmSLCB+OFb+UFINFwASAEAdpu9gaGXVyAHHFQBkHpKHc6a9dzECvADyY9sqlAMsK9W0jzxDXqeytr3mhQckxSji27TJJ5/rPmIpwJJq3HrtduriYOurv1a4i1p5HnhkG9OFymi0ReoO05cGwb+ayv4dysVygjeFmsP05f8wpZQ8fsdvfmuY9zjWSNqUtgYFVnOVReILYoBFzdQI5/GGFzNHhGbeZnopDGU29sZbscgldmC99w35VOATTycIMMcBXIfpSVGzZhA6C8hh00conln6VQ9TGgV32OEAKQC4DrBq7CJwd0ggR7Vq/rPrfgB+C3sGypY5DAAAAABJRU5ErkJggg==\"\n          alt=\"\">\n      </button>\n    </div>\n  "
                },] },
    ];
    return BsTimepickerViewComponent;
}());

//# sourceMappingURL=bs-timepicker-view.component.js.map

/***/ }),

/***/ "./node_modules/ngx-bootstrap/datepicker/themes/bs/bs-years-calendar-view.component.js":
/*!*********************************************************************************************!*\
  !*** ./node_modules/ngx-bootstrap/datepicker/themes/bs/bs-years-calendar-view.component.js ***!
  \*********************************************************************************************/
/*! exports provided: BsYearsCalendarViewComponent */
/***/ (function(module, __webpack_exports__, __webpack_require__) {

"use strict";
__webpack_require__.r(__webpack_exports__);
/* harmony export (binding) */ __webpack_require__.d(__webpack_exports__, "BsYearsCalendarViewComponent", function() { return BsYearsCalendarViewComponent; });
/* harmony import */ var _angular_core__WEBPACK_IMPORTED_MODULE_0__ = __webpack_require__(/*! @angular/core */ "./node_modules/@angular/core/fesm5/core.js");
/* harmony import */ var _engine_format_years_calendar__WEBPACK_IMPORTED_MODULE_1__ = __webpack_require__(/*! ../../engine/format-years-calendar */ "./node_modules/ngx-bootstrap/datepicker/engine/format-years-calendar.js");
/* harmony import */ var _models_index__WEBPACK_IMPORTED_MODULE_2__ = __webpack_require__(/*! ../../models/index */ "./node_modules/ngx-bootstrap/datepicker/models/index.js");



var BsYearsCalendarViewComponent = /** @class */ (function () {
    function BsYearsCalendarViewComponent() {
        this.onNavigate = new _angular_core__WEBPACK_IMPORTED_MODULE_0__["EventEmitter"]();
        this.onViewMode = new _angular_core__WEBPACK_IMPORTED_MODULE_0__["EventEmitter"]();
        this.onSelect = new _angular_core__WEBPACK_IMPORTED_MODULE_0__["EventEmitter"]();
        this.onHover = new _angular_core__WEBPACK_IMPORTED_MODULE_0__["EventEmitter"]();
    }
    BsYearsCalendarViewComponent.prototype.navigateTo = function (event) {
        var step = _models_index__WEBPACK_IMPORTED_MODULE_2__["BsNavigationDirection"].DOWN === event ? -1 : 1;
        this.onNavigate.emit({ step: { year: step * _engine_format_years_calendar__WEBPACK_IMPORTED_MODULE_1__["yearsPerCalendar"] } });
    };
    BsYearsCalendarViewComponent.prototype.viewYear = function (year) {
        this.onSelect.emit(year);
    };
    BsYearsCalendarViewComponent.prototype.hoverYear = function (cell, isHovered) {
        this.onHover.emit({ cell: cell, isHovered: isHovered });
    };
    BsYearsCalendarViewComponent.prototype.changeViewMode = function (event) {
        this.onViewMode.emit(event);
    };
    BsYearsCalendarViewComponent.decorators = [
        { type: _angular_core__WEBPACK_IMPORTED_MODULE_0__["Component"], args: [{
                    selector: 'bs-years-calendar-view',
                    template: "\n    <bs-calendar-layout>\n      <bs-datepicker-navigation-view\n        [calendar]=\"calendar\"\n        (onNavigate)=\"navigateTo($event)\"\n        (onViewMode)=\"changeViewMode($event)\"\n      ></bs-datepicker-navigation-view>\n\n      <table role=\"grid\" class=\"years\">\n        <tbody>\n        <tr *ngFor=\"let row of calendar.years\">\n          <td *ngFor=\"let year of row\" role=\"gridcell\"\n              (click)=\"viewYear(year)\"\n              (mouseenter)=\"hoverYear(year, true)\"\n              (mouseleave)=\"hoverYear(year, false)\"\n              [class.disabled]=\"year.isDisabled\"\n              [class.is-highlighted]=\"year.isHovered\">\n            <span>{{ year.label }}</span>\n          </td>\n        </tr>\n        </tbody>\n      </table>\n    </bs-calendar-layout>\n  "
                },] },
    ];
    /** @nocollapse */
    BsYearsCalendarViewComponent.propDecorators = {
        "calendar": [{ type: _angular_core__WEBPACK_IMPORTED_MODULE_0__["Input"] },],
        "onNavigate": [{ type: _angular_core__WEBPACK_IMPORTED_MODULE_0__["Output"] },],
        "onViewMode": [{ type: _angular_core__WEBPACK_IMPORTED_MODULE_0__["Output"] },],
        "onSelect": [{ type: _angular_core__WEBPACK_IMPORTED_MODULE_0__["Output"] },],
        "onHover": [{ type: _angular_core__WEBPACK_IMPORTED_MODULE_0__["Output"] },],
    };
    return BsYearsCalendarViewComponent;
}());

//# sourceMappingURL=bs-years-calendar-view.component.js.map

/***/ }),

/***/ "./node_modules/ngx-bootstrap/datepicker/utils/bs-calendar-utils.js":
/*!**************************************************************************!*\
  !*** ./node_modules/ngx-bootstrap/datepicker/utils/bs-calendar-utils.js ***!
  \**************************************************************************/
/*! exports provided: getStartingDayOfCalendar, calculateDateOffset, isMonthDisabled, isYearDisabled */
/***/ (function(module, __webpack_exports__, __webpack_require__) {

"use strict";
__webpack_require__.r(__webpack_exports__);
/* harmony export (binding) */ __webpack_require__.d(__webpack_exports__, "getStartingDayOfCalendar", function() { return getStartingDayOfCalendar; });
/* harmony export (binding) */ __webpack_require__.d(__webpack_exports__, "calculateDateOffset", function() { return calculateDateOffset; });
/* harmony export (binding) */ __webpack_require__.d(__webpack_exports__, "isMonthDisabled", function() { return isMonthDisabled; });
/* harmony export (binding) */ __webpack_require__.d(__webpack_exports__, "isYearDisabled", function() { return isYearDisabled; });
/* harmony import */ var _chronos_utils_date_getters__WEBPACK_IMPORTED_MODULE_0__ = __webpack_require__(/*! ../../chronos/utils/date-getters */ "./node_modules/ngx-bootstrap/chronos/utils/date-getters.js");
/* harmony import */ var _chronos_utils_date_setters__WEBPACK_IMPORTED_MODULE_1__ = __webpack_require__(/*! ../../chronos/utils/date-setters */ "./node_modules/ngx-bootstrap/chronos/utils/date-setters.js");
/* harmony import */ var _chronos_utils_date_compare__WEBPACK_IMPORTED_MODULE_2__ = __webpack_require__(/*! ../../chronos/utils/date-compare */ "./node_modules/ngx-bootstrap/chronos/utils/date-compare.js");
/* harmony import */ var _chronos_utils_start_end_of__WEBPACK_IMPORTED_MODULE_3__ = __webpack_require__(/*! ../../chronos/utils/start-end-of */ "./node_modules/ngx-bootstrap/chronos/utils/start-end-of.js");




function getStartingDayOfCalendar(date, options) {
    if (Object(_chronos_utils_date_getters__WEBPACK_IMPORTED_MODULE_0__["isFirstDayOfWeek"])(date, options.firstDayOfWeek)) {
        return date;
    }
    var weekDay = Object(_chronos_utils_date_getters__WEBPACK_IMPORTED_MODULE_0__["getDay"])(date);
    var offset = calculateDateOffset(weekDay, options.firstDayOfWeek);
    return Object(_chronos_utils_date_setters__WEBPACK_IMPORTED_MODULE_1__["shiftDate"])(date, { day: -offset });
}
function calculateDateOffset(weekday, startingDayOffset) {
    if (startingDayOffset === 0) {
        return weekday;
    }
    var offset = weekday - startingDayOffset % 7;
    return offset < 0 ? offset + 7 : offset;
}
function isMonthDisabled(date, min, max) {
    var minBound = min && Object(_chronos_utils_date_compare__WEBPACK_IMPORTED_MODULE_2__["isBefore"])(Object(_chronos_utils_start_end_of__WEBPACK_IMPORTED_MODULE_3__["endOf"])(date, 'month'), min, 'day');
    var maxBound = max && Object(_chronos_utils_date_compare__WEBPACK_IMPORTED_MODULE_2__["isAfter"])(Object(_chronos_utils_start_end_of__WEBPACK_IMPORTED_MODULE_3__["startOf"])(date, 'month'), max, 'day');
    return minBound || maxBound;
}
function isYearDisabled(date, min, max) {
    var minBound = min && Object(_chronos_utils_date_compare__WEBPACK_IMPORTED_MODULE_2__["isBefore"])(Object(_chronos_utils_start_end_of__WEBPACK_IMPORTED_MODULE_3__["endOf"])(date, 'year'), min, 'day');
    var maxBound = max && Object(_chronos_utils_date_compare__WEBPACK_IMPORTED_MODULE_2__["isAfter"])(Object(_chronos_utils_start_end_of__WEBPACK_IMPORTED_MODULE_3__["startOf"])(date, 'year'), max, 'day');
    return minBound || maxBound;
}
//# sourceMappingURL=bs-calendar-utils.js.map

/***/ }),

/***/ "./node_modules/ngx-bootstrap/datepicker/utils/matrix-utils.js":
/*!*********************************************************************!*\
  !*** ./node_modules/ngx-bootstrap/datepicker/utils/matrix-utils.js ***!
  \*********************************************************************/
/*! exports provided: createMatrix */
/***/ (function(module, __webpack_exports__, __webpack_require__) {

"use strict";
__webpack_require__.r(__webpack_exports__);
/* harmony export (binding) */ __webpack_require__.d(__webpack_exports__, "createMatrix", function() { return createMatrix; });
/* harmony import */ var _chronos_utils_date_setters__WEBPACK_IMPORTED_MODULE_0__ = __webpack_require__(/*! ../../chronos/utils/date-setters */ "./node_modules/ngx-bootstrap/chronos/utils/date-setters.js");

function createMatrix(options, fn) {
    var prevValue = options.initialDate;
    var matrix = new Array(options.height);
    for (var i = 0; i < options.height; i++) {
        matrix[i] = new Array(options.width);
        for (var j = 0; j < options.width; j++) {
            matrix[i][j] = fn(prevValue);
            prevValue = Object(_chronos_utils_date_setters__WEBPACK_IMPORTED_MODULE_0__["shiftDate"])(prevValue, options.shift);
        }
    }
    return matrix;
}
//# sourceMappingURL=matrix-utils.js.map

/***/ }),

/***/ "./node_modules/ngx-bootstrap/datepicker/yearpicker.component.js":
/*!***********************************************************************!*\
  !*** ./node_modules/ngx-bootstrap/datepicker/yearpicker.component.js ***!
  \***********************************************************************/
/*! exports provided: YearPickerComponent */
/***/ (function(module, __webpack_exports__, __webpack_require__) {

"use strict";
__webpack_require__.r(__webpack_exports__);
/* harmony export (binding) */ __webpack_require__.d(__webpack_exports__, "YearPickerComponent", function() { return YearPickerComponent; });
/* harmony import */ var _angular_core__WEBPACK_IMPORTED_MODULE_0__ = __webpack_require__(/*! @angular/core */ "./node_modules/@angular/core/fesm5/core.js");
/* harmony import */ var _utils_theme_provider__WEBPACK_IMPORTED_MODULE_1__ = __webpack_require__(/*! ../utils/theme-provider */ "./node_modules/ngx-bootstrap/utils/theme-provider.js");
/* harmony import */ var _datepicker_inner_component__WEBPACK_IMPORTED_MODULE_2__ = __webpack_require__(/*! ./datepicker-inner.component */ "./node_modules/ngx-bootstrap/datepicker/datepicker-inner.component.js");



var YearPickerComponent = /** @class */ (function () {
    function YearPickerComponent(datePicker) {
        this.rows = [];
        this.datePicker = datePicker;
    }
    Object.defineProperty(YearPickerComponent.prototype, "isBs4", {
        get: function () {
            return !Object(_utils_theme_provider__WEBPACK_IMPORTED_MODULE_1__["isBs3"])();
        },
        enumerable: true,
        configurable: true
    });
    YearPickerComponent.prototype.ngOnInit = function () {
        var self = this;
        this.datePicker.stepYear = { years: this.datePicker.yearRange };
        this.datePicker.setRefreshViewHandler(function () {
            var years = new Array(this.yearRange);
            var date;
            var start = self.getStartingYear(this.activeDate.getFullYear());
            for (var i = 0; i < this.yearRange; i++) {
                date = new Date(start + i, 0, 1);
                date = this.fixTimeZone(date);
                years[i] = this.createDateObject(date, this.formatYear);
                years[i].uid = this.uniqueId + '-' + i;
            }
            self.title = [years[0].label, years[this.yearRange - 1].label].join(' - ');
            self.rows = this.split(years, self.datePicker.yearColLimit);
        }, 'year');
        this.datePicker.setCompareHandler(function (date1, date2) {
            return date1.getFullYear() - date2.getFullYear();
        }, 'year');
        this.datePicker.refreshView();
    };
    YearPickerComponent.prototype.getStartingYear = function (year) {
        // todo: parseInt
        return ((year - 1) / this.datePicker.yearRange * this.datePicker.yearRange + 1);
    };
    YearPickerComponent.decorators = [
        { type: _angular_core__WEBPACK_IMPORTED_MODULE_0__["Component"], args: [{
                    selector: 'yearpicker',
                    template: "\n<table *ngIf=\"datePicker.datepickerMode==='year'\" role=\"grid\">\n  <thead>\n    <tr>\n      <th>\n        <button type=\"button\" class=\"btn btn-default btn-sm pull-left float-left\"\n                (click)=\"datePicker.move(-1)\" tabindex=\"-1\">\u2039</button>\n      </th>\n      <th [attr.colspan]=\"((datePicker.yearColLimit - 2) <= 0) ? 1 : datePicker.yearColLimit - 2\">\n        <button [id]=\"datePicker.uniqueId + '-title'\" role=\"heading\"\n                type=\"button\" class=\"btn btn-default btn-sm\"\n                (click)=\"datePicker.toggleMode(0)\"\n                [disabled]=\"datePicker.datepickerMode === datePicker.maxMode\"\n                [ngClass]=\"{disabled: datePicker.datepickerMode === datePicker.maxMode}\" tabindex=\"-1\" style=\"width:100%;\">\n          <strong>{{ title }}</strong>\n        </button>\n      </th>\n      <th>\n        <button type=\"button\" class=\"btn btn-default btn-sm pull-right float-right\"\n                (click)=\"datePicker.move(1)\" tabindex=\"-1\">\u203A</button>\n      </th>\n    </tr>\n  </thead>\n  <tbody>\n    <tr *ngFor=\"let rowz of rows\">\n      <td *ngFor=\"let dtz of rowz\" class=\"text-center\" role=\"gridcell\" [attr.id]=\"dtz.uid\">\n        <button type=\"button\" style=\"min-width:100%;\" class=\"btn btn-default\"\n                [ngClass]=\"{'btn-link': isBs4 && !dtz.selected && !datePicker.isActive(dtz), 'btn-info': dtz.selected || (isBs4 && !dtz.selected && datePicker.isActive(dtz)), disabled: dtz.disabled, active: !isBs4 && datePicker.isActive(dtz)}\"\n                [disabled]=\"dtz.disabled\"\n                (click)=\"datePicker.select(dtz.date)\" tabindex=\"-1\">\n          <span [ngClass]=\"{'text-success': isBs4 && dtz.current, 'text-info': !isBs4 && dtz.current}\">{{ dtz.label }}</span>\n        </button>\n      </td>\n    </tr>\n  </tbody>\n</table>\n  ",
                    styles: [
                        "\n    :host .btn-info .text-success {\n      color: #fff !important;\n    }\n  "
                    ]
                },] },
    ];
    /** @nocollapse */
    YearPickerComponent.ctorParameters = function () { return [
        { type: _datepicker_inner_component__WEBPACK_IMPORTED_MODULE_2__["DatePickerInnerComponent"], },
    ]; };
    return YearPickerComponent;
}());

//# sourceMappingURL=yearpicker.component.js.map

/***/ }),

/***/ "./node_modules/ngx-bootstrap/locale.js":
/*!**********************************************!*\
  !*** ./node_modules/ngx-bootstrap/locale.js ***!
  \**********************************************/
/*! exports provided: arLocale, csLocale, daLocale, deLocale, enGbLocale, esLocale, esDoLocale, esUsLocale, fiLocale, frLocale, hiLocale, huLocale, idLocale, itLocale, jaLocale, koLocale, mnLocale, nlLocale, nlBeLocale, plLocale, ptBrLocale, svLocale, ruLocale, roLocale, zhCnLocale, trLocale, heLocale, thLocale, slLocale, glLocale */
/***/ (function(module, __webpack_exports__, __webpack_require__) {

"use strict";
__webpack_require__.r(__webpack_exports__);
/* harmony import */ var _chronos_i18n_ar__WEBPACK_IMPORTED_MODULE_0__ = __webpack_require__(/*! ./chronos/i18n/ar */ "./node_modules/ngx-bootstrap/chronos/i18n/ar.js");
/* harmony reexport (safe) */ __webpack_require__.d(__webpack_exports__, "arLocale", function() { return _chronos_i18n_ar__WEBPACK_IMPORTED_MODULE_0__["arLocale"]; });

/* harmony import */ var _chronos_i18n_cs__WEBPACK_IMPORTED_MODULE_1__ = __webpack_require__(/*! ./chronos/i18n/cs */ "./node_modules/ngx-bootstrap/chronos/i18n/cs.js");
/* harmony reexport (safe) */ __webpack_require__.d(__webpack_exports__, "csLocale", function() { return _chronos_i18n_cs__WEBPACK_IMPORTED_MODULE_1__["csLocale"]; });

/* harmony import */ var _chronos_i18n_da__WEBPACK_IMPORTED_MODULE_2__ = __webpack_require__(/*! ./chronos/i18n/da */ "./node_modules/ngx-bootstrap/chronos/i18n/da.js");
/* harmony reexport (safe) */ __webpack_require__.d(__webpack_exports__, "daLocale", function() { return _chronos_i18n_da__WEBPACK_IMPORTED_MODULE_2__["daLocale"]; });

/* harmony import */ var _chronos_i18n_de__WEBPACK_IMPORTED_MODULE_3__ = __webpack_require__(/*! ./chronos/i18n/de */ "./node_modules/ngx-bootstrap/chronos/i18n/de.js");
/* harmony reexport (safe) */ __webpack_require__.d(__webpack_exports__, "deLocale", function() { return _chronos_i18n_de__WEBPACK_IMPORTED_MODULE_3__["deLocale"]; });

/* harmony import */ var _chronos_i18n_en_gb__WEBPACK_IMPORTED_MODULE_4__ = __webpack_require__(/*! ./chronos/i18n/en-gb */ "./node_modules/ngx-bootstrap/chronos/i18n/en-gb.js");
/* harmony reexport (safe) */ __webpack_require__.d(__webpack_exports__, "enGbLocale", function() { return _chronos_i18n_en_gb__WEBPACK_IMPORTED_MODULE_4__["enGbLocale"]; });

/* harmony import */ var _chronos_i18n_es__WEBPACK_IMPORTED_MODULE_5__ = __webpack_require__(/*! ./chronos/i18n/es */ "./node_modules/ngx-bootstrap/chronos/i18n/es.js");
/* harmony reexport (safe) */ __webpack_require__.d(__webpack_exports__, "esLocale", function() { return _chronos_i18n_es__WEBPACK_IMPORTED_MODULE_5__["esLocale"]; });

/* harmony import */ var _chronos_i18n_es_do__WEBPACK_IMPORTED_MODULE_6__ = __webpack_require__(/*! ./chronos/i18n/es-do */ "./node_modules/ngx-bootstrap/chronos/i18n/es-do.js");
/* harmony reexport (safe) */ __webpack_require__.d(__webpack_exports__, "esDoLocale", function() { return _chronos_i18n_es_do__WEBPACK_IMPORTED_MODULE_6__["esDoLocale"]; });

/* harmony import */ var _chronos_i18n_es_us__WEBPACK_IMPORTED_MODULE_7__ = __webpack_require__(/*! ./chronos/i18n/es-us */ "./node_modules/ngx-bootstrap/chronos/i18n/es-us.js");
/* harmony reexport (safe) */ __webpack_require__.d(__webpack_exports__, "esUsLocale", function() { return _chronos_i18n_es_us__WEBPACK_IMPORTED_MODULE_7__["esUsLocale"]; });

/* harmony import */ var _chronos_i18n_fi__WEBPACK_IMPORTED_MODULE_8__ = __webpack_require__(/*! ./chronos/i18n/fi */ "./node_modules/ngx-bootstrap/chronos/i18n/fi.js");
/* harmony reexport (safe) */ __webpack_require__.d(__webpack_exports__, "fiLocale", function() { return _chronos_i18n_fi__WEBPACK_IMPORTED_MODULE_8__["fiLocale"]; });

/* harmony import */ var _chronos_i18n_fr__WEBPACK_IMPORTED_MODULE_9__ = __webpack_require__(/*! ./chronos/i18n/fr */ "./node_modules/ngx-bootstrap/chronos/i18n/fr.js");
/* harmony reexport (safe) */ __webpack_require__.d(__webpack_exports__, "frLocale", function() { return _chronos_i18n_fr__WEBPACK_IMPORTED_MODULE_9__["frLocale"]; });

/* harmony import */ var _chronos_i18n_hi__WEBPACK_IMPORTED_MODULE_10__ = __webpack_require__(/*! ./chronos/i18n/hi */ "./node_modules/ngx-bootstrap/chronos/i18n/hi.js");
/* harmony reexport (safe) */ __webpack_require__.d(__webpack_exports__, "hiLocale", function() { return _chronos_i18n_hi__WEBPACK_IMPORTED_MODULE_10__["hiLocale"]; });

/* harmony import */ var _chronos_i18n_hu__WEBPACK_IMPORTED_MODULE_11__ = __webpack_require__(/*! ./chronos/i18n/hu */ "./node_modules/ngx-bootstrap/chronos/i18n/hu.js");
/* harmony reexport (safe) */ __webpack_require__.d(__webpack_exports__, "huLocale", function() { return _chronos_i18n_hu__WEBPACK_IMPORTED_MODULE_11__["huLocale"]; });

/* harmony import */ var _chronos_i18n_id__WEBPACK_IMPORTED_MODULE_12__ = __webpack_require__(/*! ./chronos/i18n/id */ "./node_modules/ngx-bootstrap/chronos/i18n/id.js");
/* harmony reexport (safe) */ __webpack_require__.d(__webpack_exports__, "idLocale", function() { return _chronos_i18n_id__WEBPACK_IMPORTED_MODULE_12__["idLocale"]; });

/* harmony import */ var _chronos_i18n_it__WEBPACK_IMPORTED_MODULE_13__ = __webpack_require__(/*! ./chronos/i18n/it */ "./node_modules/ngx-bootstrap/chronos/i18n/it.js");
/* harmony reexport (safe) */ __webpack_require__.d(__webpack_exports__, "itLocale", function() { return _chronos_i18n_it__WEBPACK_IMPORTED_MODULE_13__["itLocale"]; });

/* harmony import */ var _chronos_i18n_ja__WEBPACK_IMPORTED_MODULE_14__ = __webpack_require__(/*! ./chronos/i18n/ja */ "./node_modules/ngx-bootstrap/chronos/i18n/ja.js");
/* harmony reexport (safe) */ __webpack_require__.d(__webpack_exports__, "jaLocale", function() { return _chronos_i18n_ja__WEBPACK_IMPORTED_MODULE_14__["jaLocale"]; });

/* harmony import */ var _chronos_i18n_ko__WEBPACK_IMPORTED_MODULE_15__ = __webpack_require__(/*! ./chronos/i18n/ko */ "./node_modules/ngx-bootstrap/chronos/i18n/ko.js");
/* harmony reexport (safe) */ __webpack_require__.d(__webpack_exports__, "koLocale", function() { return _chronos_i18n_ko__WEBPACK_IMPORTED_MODULE_15__["koLocale"]; });

/* harmony import */ var _chronos_i18n_mn__WEBPACK_IMPORTED_MODULE_16__ = __webpack_require__(/*! ./chronos/i18n/mn */ "./node_modules/ngx-bootstrap/chronos/i18n/mn.js");
/* harmony reexport (safe) */ __webpack_require__.d(__webpack_exports__, "mnLocale", function() { return _chronos_i18n_mn__WEBPACK_IMPORTED_MODULE_16__["mnLocale"]; });

/* harmony import */ var _chronos_i18n_nl__WEBPACK_IMPORTED_MODULE_17__ = __webpack_require__(/*! ./chronos/i18n/nl */ "./node_modules/ngx-bootstrap/chronos/i18n/nl.js");
/* harmony reexport (safe) */ __webpack_require__.d(__webpack_exports__, "nlLocale", function() { return _chronos_i18n_nl__WEBPACK_IMPORTED_MODULE_17__["nlLocale"]; });

/* harmony import */ var _chronos_i18n_nl_be__WEBPACK_IMPORTED_MODULE_18__ = __webpack_require__(/*! ./chronos/i18n/nl-be */ "./node_modules/ngx-bootstrap/chronos/i18n/nl-be.js");
/* harmony reexport (safe) */ __webpack_require__.d(__webpack_exports__, "nlBeLocale", function() { return _chronos_i18n_nl_be__WEBPACK_IMPORTED_MODULE_18__["nlBeLocale"]; });

/* harmony import */ var _chronos_i18n_pl__WEBPACK_IMPORTED_MODULE_19__ = __webpack_require__(/*! ./chronos/i18n/pl */ "./node_modules/ngx-bootstrap/chronos/i18n/pl.js");
/* harmony reexport (safe) */ __webpack_require__.d(__webpack_exports__, "plLocale", function() { return _chronos_i18n_pl__WEBPACK_IMPORTED_MODULE_19__["plLocale"]; });

/* harmony import */ var _chronos_i18n_pt_br__WEBPACK_IMPORTED_MODULE_20__ = __webpack_require__(/*! ./chronos/i18n/pt-br */ "./node_modules/ngx-bootstrap/chronos/i18n/pt-br.js");
/* harmony reexport (safe) */ __webpack_require__.d(__webpack_exports__, "ptBrLocale", function() { return _chronos_i18n_pt_br__WEBPACK_IMPORTED_MODULE_20__["ptBrLocale"]; });

/* harmony import */ var _chronos_i18n_sv__WEBPACK_IMPORTED_MODULE_21__ = __webpack_require__(/*! ./chronos/i18n/sv */ "./node_modules/ngx-bootstrap/chronos/i18n/sv.js");
/* harmony reexport (safe) */ __webpack_require__.d(__webpack_exports__, "svLocale", function() { return _chronos_i18n_sv__WEBPACK_IMPORTED_MODULE_21__["svLocale"]; });

/* harmony import */ var _chronos_i18n_ru__WEBPACK_IMPORTED_MODULE_22__ = __webpack_require__(/*! ./chronos/i18n/ru */ "./node_modules/ngx-bootstrap/chronos/i18n/ru.js");
/* harmony reexport (safe) */ __webpack_require__.d(__webpack_exports__, "ruLocale", function() { return _chronos_i18n_ru__WEBPACK_IMPORTED_MODULE_22__["ruLocale"]; });

/* harmony import */ var _chronos_i18n_ro__WEBPACK_IMPORTED_MODULE_23__ = __webpack_require__(/*! ./chronos/i18n/ro */ "./node_modules/ngx-bootstrap/chronos/i18n/ro.js");
/* harmony reexport (safe) */ __webpack_require__.d(__webpack_exports__, "roLocale", function() { return _chronos_i18n_ro__WEBPACK_IMPORTED_MODULE_23__["roLocale"]; });

/* harmony import */ var _chronos_i18n_zh_cn__WEBPACK_IMPORTED_MODULE_24__ = __webpack_require__(/*! ./chronos/i18n/zh-cn */ "./node_modules/ngx-bootstrap/chronos/i18n/zh-cn.js");
/* harmony reexport (safe) */ __webpack_require__.d(__webpack_exports__, "zhCnLocale", function() { return _chronos_i18n_zh_cn__WEBPACK_IMPORTED_MODULE_24__["zhCnLocale"]; });

/* harmony import */ var _chronos_i18n_tr__WEBPACK_IMPORTED_MODULE_25__ = __webpack_require__(/*! ./chronos/i18n/tr */ "./node_modules/ngx-bootstrap/chronos/i18n/tr.js");
/* harmony reexport (safe) */ __webpack_require__.d(__webpack_exports__, "trLocale", function() { return _chronos_i18n_tr__WEBPACK_IMPORTED_MODULE_25__["trLocale"]; });

/* harmony import */ var _chronos_i18n_he__WEBPACK_IMPORTED_MODULE_26__ = __webpack_require__(/*! ./chronos/i18n/he */ "./node_modules/ngx-bootstrap/chronos/i18n/he.js");
/* harmony reexport (safe) */ __webpack_require__.d(__webpack_exports__, "heLocale", function() { return _chronos_i18n_he__WEBPACK_IMPORTED_MODULE_26__["heLocale"]; });

/* harmony import */ var _chronos_i18n_th__WEBPACK_IMPORTED_MODULE_27__ = __webpack_require__(/*! ./chronos/i18n/th */ "./node_modules/ngx-bootstrap/chronos/i18n/th.js");
/* harmony reexport (safe) */ __webpack_require__.d(__webpack_exports__, "thLocale", function() { return _chronos_i18n_th__WEBPACK_IMPORTED_MODULE_27__["thLocale"]; });

/* harmony import */ var _chronos_i18n_sl__WEBPACK_IMPORTED_MODULE_28__ = __webpack_require__(/*! ./chronos/i18n/sl */ "./node_modules/ngx-bootstrap/chronos/i18n/sl.js");
/* harmony reexport (safe) */ __webpack_require__.d(__webpack_exports__, "slLocale", function() { return _chronos_i18n_sl__WEBPACK_IMPORTED_MODULE_28__["slLocale"]; });

/* harmony import */ var _chronos_i18n_gl__WEBPACK_IMPORTED_MODULE_29__ = __webpack_require__(/*! ./chronos/i18n/gl */ "./node_modules/ngx-bootstrap/chronos/i18n/gl.js");
/* harmony reexport (safe) */ __webpack_require__.d(__webpack_exports__, "glLocale", function() { return _chronos_i18n_gl__WEBPACK_IMPORTED_MODULE_29__["glLocale"]; });































//# sourceMappingURL=locale.js.map

/***/ }),

/***/ "./node_modules/ngx-bootstrap/mini-ngrx/state.class.js":
/*!*************************************************************!*\
  !*** ./node_modules/ngx-bootstrap/mini-ngrx/state.class.js ***!
  \*************************************************************/
/*! exports provided: MiniState */
/***/ (function(module, __webpack_exports__, __webpack_require__) {

"use strict";
__webpack_require__.r(__webpack_exports__);
/* harmony export (binding) */ __webpack_require__.d(__webpack_exports__, "MiniState", function() { return MiniState; });
/* harmony import */ var rxjs__WEBPACK_IMPORTED_MODULE_0__ = __webpack_require__(/*! rxjs */ "./node_modules/rxjs/_esm5/index.js");
/* harmony import */ var rxjs_operators__WEBPACK_IMPORTED_MODULE_1__ = __webpack_require__(/*! rxjs/operators */ "./node_modules/rxjs/_esm5/operators/index.js");
var __extends = (undefined && undefined.__extends) || (function () {
    var extendStatics = Object.setPrototypeOf ||
        ({ __proto__: [] } instanceof Array && function (d, b) { d.__proto__ = b; }) ||
        function (d, b) { for (var p in b) if (b.hasOwnProperty(p)) d[p] = b[p]; };
    return function (d, b) {
        extendStatics(d, b);
        function __() { this.constructor = d; }
        d.prototype = b === null ? Object.create(b) : (__.prototype = b.prototype, new __());
    };
})();


var MiniState = /** @class */ (function (_super) {
    __extends(MiniState, _super);
    function MiniState(_initialState, actionsDispatcher$, reducer) {
        var _this = _super.call(this, _initialState) || this;
        var actionInQueue$ = actionsDispatcher$.pipe(Object(rxjs_operators__WEBPACK_IMPORTED_MODULE_1__["observeOn"])(rxjs__WEBPACK_IMPORTED_MODULE_0__["queueScheduler"]));
        var state$ = actionInQueue$.pipe(Object(rxjs_operators__WEBPACK_IMPORTED_MODULE_1__["scan"])(function (state, action) {
            if (!action) {
                return state;
            }
            return reducer(state, action);
        }, _initialState));
        state$.subscribe(function (value) { return _this.next(value); });
        return _this;
    }
    return MiniState;
}(rxjs__WEBPACK_IMPORTED_MODULE_0__["BehaviorSubject"]));

//# sourceMappingURL=state.class.js.map

/***/ }),

/***/ "./node_modules/ngx-bootstrap/mini-ngrx/store.class.js":
/*!*************************************************************!*\
  !*** ./node_modules/ngx-bootstrap/mini-ngrx/store.class.js ***!
  \*************************************************************/
/*! exports provided: MiniStore */
/***/ (function(module, __webpack_exports__, __webpack_require__) {

"use strict";
__webpack_require__.r(__webpack_exports__);
/* harmony export (binding) */ __webpack_require__.d(__webpack_exports__, "MiniStore", function() { return MiniStore; });
/* harmony import */ var rxjs__WEBPACK_IMPORTED_MODULE_0__ = __webpack_require__(/*! rxjs */ "./node_modules/rxjs/_esm5/index.js");
/* harmony import */ var rxjs_operators__WEBPACK_IMPORTED_MODULE_1__ = __webpack_require__(/*! rxjs/operators */ "./node_modules/rxjs/_esm5/operators/index.js");
var __extends = (undefined && undefined.__extends) || (function () {
    var extendStatics = Object.setPrototypeOf ||
        ({ __proto__: [] } instanceof Array && function (d, b) { d.__proto__ = b; }) ||
        function (d, b) { for (var p in b) if (b.hasOwnProperty(p)) d[p] = b[p]; };
    return function (d, b) {
        extendStatics(d, b);
        function __() { this.constructor = d; }
        d.prototype = b === null ? Object.create(b) : (__.prototype = b.prototype, new __());
    };
})();


var MiniStore = /** @class */ (function (_super) {
    __extends(MiniStore, _super);
    function MiniStore(_dispatcher, _reducer, state$) {
        var _this = _super.call(this) || this;
        _this._dispatcher = _dispatcher;
        _this._reducer = _reducer;
        _this.source = state$;
        return _this;
    }
    MiniStore.prototype.select = function (pathOrMapFn) {
        var mapped$ = this.source.pipe(Object(rxjs_operators__WEBPACK_IMPORTED_MODULE_1__["map"])(pathOrMapFn));
        return mapped$.pipe(Object(rxjs_operators__WEBPACK_IMPORTED_MODULE_1__["distinctUntilChanged"])());
    };
    MiniStore.prototype.lift = function (operator) {
        var store = new MiniStore(this._dispatcher, this._reducer, this);
        store.operator = operator;
        return store;
    };
    MiniStore.prototype.dispatch = function (action) {
        this._dispatcher.next(action);
    };
    MiniStore.prototype.next = function (action) {
        this._dispatcher.next(action);
    };
    MiniStore.prototype.error = function (err) {
        this._dispatcher.error(err);
    };
    MiniStore.prototype.complete = function () {
        /*noop*/
    };
    return MiniStore;
}(rxjs__WEBPACK_IMPORTED_MODULE_0__["Observable"]));

//# sourceMappingURL=store.class.js.map

/***/ }),

/***/ "./src/app/services/api/api/api.ts":
/*!*****************************************!*\
  !*** ./src/app/services/api/api/api.ts ***!
  \*****************************************/
/*! exports provided: QueryService, APIS */
/***/ (function(module, __webpack_exports__, __webpack_require__) {

"use strict";
__webpack_require__.r(__webpack_exports__);
/* harmony export (binding) */ __webpack_require__.d(__webpack_exports__, "APIS", function() { return APIS; });
/* harmony import */ var _query_service__WEBPACK_IMPORTED_MODULE_0__ = __webpack_require__(/*! ./query.service */ "./src/app/services/api/api/query.service.ts");
/* harmony reexport (safe) */ __webpack_require__.d(__webpack_exports__, "QueryService", function() { return _query_service__WEBPACK_IMPORTED_MODULE_0__["QueryService"]; });



var APIS = [_query_service__WEBPACK_IMPORTED_MODULE_0__["QueryService"]];


/***/ }),

/***/ "./src/app/services/api/index.ts":
/*!***************************************!*\
  !*** ./src/app/services/api/index.ts ***!
  \***************************************/
/*! exports provided: BASE_PATH, COLLECTION_FORMATS, Configuration, QueryService, APIS, LogLevelEnum, LogLevelQuantity, LogLevelTimes, NodeLogItem, NodeStatusItemValue */
/***/ (function(module, __webpack_exports__, __webpack_require__) {

"use strict";
__webpack_require__.r(__webpack_exports__);
/* harmony import */ var _api_api__WEBPACK_IMPORTED_MODULE_0__ = __webpack_require__(/*! ./api/api */ "./src/app/services/api/api/api.ts");
/* harmony reexport (safe) */ __webpack_require__.d(__webpack_exports__, "QueryService", function() { return _api_api__WEBPACK_IMPORTED_MODULE_0__["QueryService"]; });

/* harmony reexport (safe) */ __webpack_require__.d(__webpack_exports__, "APIS", function() { return _api_api__WEBPACK_IMPORTED_MODULE_0__["APIS"]; });

/* harmony import */ var _model_models__WEBPACK_IMPORTED_MODULE_1__ = __webpack_require__(/*! ./model/models */ "./src/app/services/api/model/models.ts");
/* harmony reexport (safe) */ __webpack_require__.d(__webpack_exports__, "LogLevelEnum", function() { return _model_models__WEBPACK_IMPORTED_MODULE_1__["LogLevelEnum"]; });

/* harmony reexport (safe) */ __webpack_require__.d(__webpack_exports__, "LogLevelQuantity", function() { return _model_models__WEBPACK_IMPORTED_MODULE_1__["LogLevelQuantity"]; });

/* harmony reexport (safe) */ __webpack_require__.d(__webpack_exports__, "LogLevelTimes", function() { return _model_models__WEBPACK_IMPORTED_MODULE_1__["LogLevelTimes"]; });

/* harmony reexport (safe) */ __webpack_require__.d(__webpack_exports__, "NodeLogItem", function() { return _model_models__WEBPACK_IMPORTED_MODULE_1__["NodeLogItem"]; });

/* harmony reexport (safe) */ __webpack_require__.d(__webpack_exports__, "NodeStatusItemValue", function() { return _model_models__WEBPACK_IMPORTED_MODULE_1__["NodeStatusItemValue"]; });

/* harmony import */ var _variables__WEBPACK_IMPORTED_MODULE_2__ = __webpack_require__(/*! ./variables */ "./src/app/services/api/variables.ts");
/* harmony reexport (safe) */ __webpack_require__.d(__webpack_exports__, "BASE_PATH", function() { return _variables__WEBPACK_IMPORTED_MODULE_2__["BASE_PATH"]; });

/* harmony reexport (safe) */ __webpack_require__.d(__webpack_exports__, "COLLECTION_FORMATS", function() { return _variables__WEBPACK_IMPORTED_MODULE_2__["COLLECTION_FORMATS"]; });

/* harmony import */ var _configuration__WEBPACK_IMPORTED_MODULE_3__ = __webpack_require__(/*! ./configuration */ "./src/app/services/api/configuration.ts");
/* harmony reexport (safe) */ __webpack_require__.d(__webpack_exports__, "Configuration", function() { return _configuration__WEBPACK_IMPORTED_MODULE_3__["Configuration"]; });







/***/ }),

/***/ "./src/app/services/api/model/logLevelQuantity.ts":
/*!********************************************************!*\
  !*** ./src/app/services/api/model/logLevelQuantity.ts ***!
  \********************************************************/
/*! exports provided: LogLevelQuantity */
/***/ (function(module, __webpack_exports__, __webpack_require__) {

"use strict";
__webpack_require__.r(__webpack_exports__);
/* harmony export (binding) */ __webpack_require__.d(__webpack_exports__, "LogLevelQuantity", function() { return LogLevelQuantity; });
/**
 * TWCore Diagnostics Api
 * TWCore diagnostics api
 *
 * OpenAPI spec version: v1
 *
 *
 * NOTE: This class is auto generated by the swagger code generator program.
 * https://github.com/swagger-api/swagger-codegen.git
 * Do not edit the class manually.
 */
var LogLevelQuantity;
(function (LogLevelQuantity) {
    LogLevelQuantity.NameEnum = {
        Error: 'Error',
        Warning: 'Warning',
        InfoBasic: 'InfoBasic',
        InfoMedium: 'InfoMedium',
        InfoDetail: 'InfoDetail',
        Debug: 'Debug',
        Verbose: 'Verbose',
        Stats: 'Stats',
        LibDebug: 'LibDebug',
        LibVerbose: 'LibVerbose'
    };
})(LogLevelQuantity || (LogLevelQuantity = {}));


/***/ }),

/***/ "./src/app/services/api/model/logLevelTimes.ts":
/*!*****************************************************!*\
  !*** ./src/app/services/api/model/logLevelTimes.ts ***!
  \*****************************************************/
/*! exports provided: LogLevelTimes */
/***/ (function(module, __webpack_exports__, __webpack_require__) {

"use strict";
__webpack_require__.r(__webpack_exports__);
/* harmony export (binding) */ __webpack_require__.d(__webpack_exports__, "LogLevelTimes", function() { return LogLevelTimes; });
var LogLevelTimes;
(function (LogLevelTimes) {
    LogLevelTimes.NameEnum = {
        Error: 'Error',
        Warning: 'Warning',
        InfoBasic: 'InfoBasic',
        InfoMedium: 'InfoMedium',
        InfoDetail: 'InfoDetail',
        Debug: 'Debug',
        Verbose: 'Verbose',
        Stats: 'Stats',
        LibDebug: 'LibDebug',
        LibVerbose: 'LibVerbose'
    };
})(LogLevelTimes || (LogLevelTimes = {}));


/***/ }),

/***/ "./src/app/services/api/model/loglevel.ts":
/*!************************************************!*\
  !*** ./src/app/services/api/model/loglevel.ts ***!
  \************************************************/
/*! exports provided: LogLevelEnum */
/***/ (function(module, __webpack_exports__, __webpack_require__) {

"use strict";
__webpack_require__.r(__webpack_exports__);
/* harmony export (binding) */ __webpack_require__.d(__webpack_exports__, "LogLevelEnum", function() { return LogLevelEnum; });
var LogLevelEnum = {
    Error: 'Error',
    Warning: 'Warning',
    InfoBasic: 'InfoBasic',
    InfoMedium: 'InfoMedium',
    InfoDetail: 'InfoDetail',
    Debug: 'Debug',
    Verbose: 'Verbose',
    Stats: 'Stats',
    LibDebug: 'LibDebug',
    LibVerbose: 'LibVerbose'
};


/***/ }),

/***/ "./src/app/services/api/model/models.ts":
/*!**********************************************!*\
  !*** ./src/app/services/api/model/models.ts ***!
  \**********************************************/
/*! exports provided: LogLevelEnum, LogLevelQuantity, LogLevelTimes, NodeLogItem, NodeStatusItemValue */
/***/ (function(module, __webpack_exports__, __webpack_require__) {

"use strict";
__webpack_require__.r(__webpack_exports__);
/* harmony import */ var _loglevel__WEBPACK_IMPORTED_MODULE_0__ = __webpack_require__(/*! ./loglevel */ "./src/app/services/api/model/loglevel.ts");
/* harmony reexport (safe) */ __webpack_require__.d(__webpack_exports__, "LogLevelEnum", function() { return _loglevel__WEBPACK_IMPORTED_MODULE_0__["LogLevelEnum"]; });

/* harmony import */ var _logLevelQuantity__WEBPACK_IMPORTED_MODULE_1__ = __webpack_require__(/*! ./logLevelQuantity */ "./src/app/services/api/model/logLevelQuantity.ts");
/* harmony reexport (safe) */ __webpack_require__.d(__webpack_exports__, "LogLevelQuantity", function() { return _logLevelQuantity__WEBPACK_IMPORTED_MODULE_1__["LogLevelQuantity"]; });

/* harmony import */ var _logLevelTimes__WEBPACK_IMPORTED_MODULE_2__ = __webpack_require__(/*! ./logLevelTimes */ "./src/app/services/api/model/logLevelTimes.ts");
/* harmony reexport (safe) */ __webpack_require__.d(__webpack_exports__, "LogLevelTimes", function() { return _logLevelTimes__WEBPACK_IMPORTED_MODULE_2__["LogLevelTimes"]; });

/* harmony import */ var _nodeLogItem__WEBPACK_IMPORTED_MODULE_3__ = __webpack_require__(/*! ./nodeLogItem */ "./src/app/services/api/model/nodeLogItem.ts");
/* harmony reexport (safe) */ __webpack_require__.d(__webpack_exports__, "NodeLogItem", function() { return _nodeLogItem__WEBPACK_IMPORTED_MODULE_3__["NodeLogItem"]; });

/* harmony import */ var _nodeStatusItemValue__WEBPACK_IMPORTED_MODULE_4__ = __webpack_require__(/*! ./nodeStatusItemValue */ "./src/app/services/api/model/nodeStatusItemValue.ts");
/* harmony reexport (safe) */ __webpack_require__.d(__webpack_exports__, "NodeStatusItemValue", function() { return _nodeStatusItemValue__WEBPACK_IMPORTED_MODULE_4__["NodeStatusItemValue"]; });








/***/ }),

/***/ "./src/app/services/api/model/nodeLogItem.ts":
/*!***************************************************!*\
  !*** ./src/app/services/api/model/nodeLogItem.ts ***!
  \***************************************************/
/*! exports provided: NodeLogItem */
/***/ (function(module, __webpack_exports__, __webpack_require__) {

"use strict";
__webpack_require__.r(__webpack_exports__);
/* harmony export (binding) */ __webpack_require__.d(__webpack_exports__, "NodeLogItem", function() { return NodeLogItem; });
var NodeLogItem;
(function (NodeLogItem) {
    NodeLogItem.LevelEnum = {
        Error: 'Error',
        Warning: 'Warning',
        InfoBasic: 'InfoBasic',
        InfoMedium: 'InfoMedium',
        InfoDetail: 'InfoDetail',
        Debug: 'Debug',
        Verbose: 'Verbose',
        Stats: 'Stats',
        LibDebug: 'LibDebug',
        LibVerbose: 'LibVerbose'
    };
})(NodeLogItem || (NodeLogItem = {}));


/***/ }),

/***/ "./src/app/services/api/model/nodeStatusItemValue.ts":
/*!***********************************************************!*\
  !*** ./src/app/services/api/model/nodeStatusItemValue.ts ***!
  \***********************************************************/
/*! exports provided: NodeStatusItemValue */
/***/ (function(module, __webpack_exports__, __webpack_require__) {

"use strict";
__webpack_require__.r(__webpack_exports__);
/* harmony export (binding) */ __webpack_require__.d(__webpack_exports__, "NodeStatusItemValue", function() { return NodeStatusItemValue; });
/**
 * TWCore Diagnostics Api
 * TWCore diagnostics api
 *
 * OpenAPI spec version: v1
 *
 *
 * NOTE: This class is auto generated by the swagger code generator program.
 * https://github.com/swagger-api/swagger-codegen.git
 * Do not edit the class manually.
 */
var NodeStatusItemValue;
(function (NodeStatusItemValue) {
    NodeStatusItemValue.TypeEnum = {
        Text: 'Text',
        Number: 'Number',
        Date: 'Date',
        Time: 'Time',
        Array: 'Array'
    };
})(NodeStatusItemValue || (NodeStatusItemValue = {}));


/***/ }),

/***/ "./src/app/views/diagnostics/diagnostics-routing.module.ts":
/*!*****************************************************************!*\
  !*** ./src/app/views/diagnostics/diagnostics-routing.module.ts ***!
  \*****************************************************************/
/*! exports provided: DiagnosticsRoutingModule */
/***/ (function(module, __webpack_exports__, __webpack_require__) {

"use strict";
__webpack_require__.r(__webpack_exports__);
/* harmony export (binding) */ __webpack_require__.d(__webpack_exports__, "DiagnosticsRoutingModule", function() { return DiagnosticsRoutingModule; });
/* harmony import */ var _angular_core__WEBPACK_IMPORTED_MODULE_0__ = __webpack_require__(/*! @angular/core */ "./node_modules/@angular/core/fesm5/core.js");
/* harmony import */ var _angular_router__WEBPACK_IMPORTED_MODULE_1__ = __webpack_require__(/*! @angular/router */ "./node_modules/@angular/router/fesm5/router.js");
/* harmony import */ var _logs_component__WEBPACK_IMPORTED_MODULE_2__ = __webpack_require__(/*! ./logs.component */ "./src/app/views/diagnostics/logs.component.ts");
/* harmony import */ var _traces_component__WEBPACK_IMPORTED_MODULE_3__ = __webpack_require__(/*! ./traces.component */ "./src/app/views/diagnostics/traces.component.ts");
/* harmony import */ var _tracedetails_component__WEBPACK_IMPORTED_MODULE_4__ = __webpack_require__(/*! ./tracedetails.component */ "./src/app/views/diagnostics/tracedetails.component.ts");
/* harmony import */ var _status_component__WEBPACK_IMPORTED_MODULE_5__ = __webpack_require__(/*! ./status.component */ "./src/app/views/diagnostics/status.component.ts");
/* harmony import */ var _search_component__WEBPACK_IMPORTED_MODULE_6__ = __webpack_require__(/*! ./search.component */ "./src/app/views/diagnostics/search.component.ts");
var __decorate = (undefined && undefined.__decorate) || function (decorators, target, key, desc) {
    var c = arguments.length, r = c < 3 ? target : desc === null ? desc = Object.getOwnPropertyDescriptor(target, key) : desc, d;
    if (typeof Reflect === "object" && typeof Reflect.decorate === "function") r = Reflect.decorate(decorators, target, key, desc);
    else for (var i = decorators.length - 1; i >= 0; i--) if (d = decorators[i]) r = (c < 3 ? d(r) : c > 3 ? d(target, key, r) : d(target, key)) || r;
    return c > 3 && r && Object.defineProperty(target, key, r), r;
};







var routes = [
    {
        path: '',
        children: [
            {
                path: 'search',
                component: _search_component__WEBPACK_IMPORTED_MODULE_6__["SearchComponent"],
                data: {
                    title: 'Search'
                }
            },
            {
                path: 'logs',
                component: _logs_component__WEBPACK_IMPORTED_MODULE_2__["LogsComponent"],
                data: {
                    title: 'Logs'
                }
            },
            {
                path: 'traces',
                component: _traces_component__WEBPACK_IMPORTED_MODULE_3__["TracesComponent"],
                data: {
                    title: 'Traces'
                }
            },
            {
                path: 'traces/:group',
                component: _tracedetails_component__WEBPACK_IMPORTED_MODULE_4__["TraceDetailsComponent"],
                data: {
                    title: 'Trace Group Details'
                }
            },
            {
                path: 'status',
                component: _status_component__WEBPACK_IMPORTED_MODULE_5__["StatusComponent"],
                data: {
                    title: 'Status'
                }
            }
        ]
    }
];
var DiagnosticsRoutingModule = /** @class */ (function () {
    function DiagnosticsRoutingModule() {
    }
    DiagnosticsRoutingModule = __decorate([
        Object(_angular_core__WEBPACK_IMPORTED_MODULE_0__["NgModule"])({
            imports: [_angular_router__WEBPACK_IMPORTED_MODULE_1__["RouterModule"].forChild(routes)],
            exports: [_angular_router__WEBPACK_IMPORTED_MODULE_1__["RouterModule"]]
        })
    ], DiagnosticsRoutingModule);
    return DiagnosticsRoutingModule;
}());



/***/ }),

/***/ "./src/app/views/diagnostics/diagnostics.module.ts":
/*!*********************************************************!*\
  !*** ./src/app/views/diagnostics/diagnostics.module.ts ***!
  \*********************************************************/
/*! exports provided: DiagnosticsModule */
/***/ (function(module, __webpack_exports__, __webpack_require__) {

"use strict";
__webpack_require__.r(__webpack_exports__);
/* harmony export (binding) */ __webpack_require__.d(__webpack_exports__, "DiagnosticsModule", function() { return DiagnosticsModule; });
/* harmony import */ var _angular_common__WEBPACK_IMPORTED_MODULE_0__ = __webpack_require__(/*! @angular/common */ "./node_modules/@angular/common/fesm5/common.js");
/* harmony import */ var _angular_forms__WEBPACK_IMPORTED_MODULE_1__ = __webpack_require__(/*! @angular/forms */ "./node_modules/@angular/forms/fesm5/forms.js");
/* harmony import */ var _angular_core__WEBPACK_IMPORTED_MODULE_2__ = __webpack_require__(/*! @angular/core */ "./node_modules/@angular/core/fesm5/core.js");
/* harmony import */ var ngx_bootstrap_tabs__WEBPACK_IMPORTED_MODULE_3__ = __webpack_require__(/*! ngx-bootstrap/tabs */ "./node_modules/ngx-bootstrap/tabs/index.js");
/* harmony import */ var ngx_bootstrap_carousel__WEBPACK_IMPORTED_MODULE_4__ = __webpack_require__(/*! ngx-bootstrap/carousel */ "./node_modules/ngx-bootstrap/carousel/index.js");
/* harmony import */ var ngx_bootstrap_collapse__WEBPACK_IMPORTED_MODULE_5__ = __webpack_require__(/*! ngx-bootstrap/collapse */ "./node_modules/ngx-bootstrap/collapse/index.js");
/* harmony import */ var ngx_bootstrap_dropdown__WEBPACK_IMPORTED_MODULE_6__ = __webpack_require__(/*! ngx-bootstrap/dropdown */ "./node_modules/ngx-bootstrap/dropdown/index.js");
/* harmony import */ var ngx_bootstrap_pagination__WEBPACK_IMPORTED_MODULE_7__ = __webpack_require__(/*! ngx-bootstrap/pagination */ "./node_modules/ngx-bootstrap/pagination/index.js");
/* harmony import */ var ngx_bootstrap_popover__WEBPACK_IMPORTED_MODULE_8__ = __webpack_require__(/*! ngx-bootstrap/popover */ "./node_modules/ngx-bootstrap/popover/index.js");
/* harmony import */ var ngx_bootstrap_progressbar__WEBPACK_IMPORTED_MODULE_9__ = __webpack_require__(/*! ngx-bootstrap/progressbar */ "./node_modules/ngx-bootstrap/progressbar/index.js");
/* harmony import */ var ngx_bootstrap_tooltip__WEBPACK_IMPORTED_MODULE_10__ = __webpack_require__(/*! ngx-bootstrap/tooltip */ "./node_modules/ngx-bootstrap/tooltip/index.js");
/* harmony import */ var ngx_bootstrap_datepicker__WEBPACK_IMPORTED_MODULE_11__ = __webpack_require__(/*! ngx-bootstrap/datepicker */ "./node_modules/ngx-bootstrap/datepicker/index.js");
/* harmony import */ var ngx_bootstrap_alert__WEBPACK_IMPORTED_MODULE_12__ = __webpack_require__(/*! ngx-bootstrap/alert */ "./node_modules/ngx-bootstrap/alert/index.js");
/* harmony import */ var _nomadreservations_ngx_codemirror__WEBPACK_IMPORTED_MODULE_13__ = __webpack_require__(/*! @nomadreservations/ngx-codemirror */ "./node_modules/@nomadreservations/ngx-codemirror/fesm5/nomadreservations-ngx-codemirror.js");
/* harmony import */ var _diagnostics_routing_module__WEBPACK_IMPORTED_MODULE_14__ = __webpack_require__(/*! ./diagnostics-routing.module */ "./src/app/views/diagnostics/diagnostics-routing.module.ts");
/* harmony import */ var ngx_bootstrap_modal__WEBPACK_IMPORTED_MODULE_15__ = __webpack_require__(/*! ngx-bootstrap/modal */ "./node_modules/ngx-bootstrap/modal/index.js");
/* harmony import */ var ng2_charts_ng2_charts__WEBPACK_IMPORTED_MODULE_16__ = __webpack_require__(/*! ng2-charts/ng2-charts */ "./node_modules/ng2-charts/ng2-charts.js");
/* harmony import */ var ng2_charts_ng2_charts__WEBPACK_IMPORTED_MODULE_16___default = /*#__PURE__*/__webpack_require__.n(ng2_charts_ng2_charts__WEBPACK_IMPORTED_MODULE_16__);
/* harmony import */ var _logs_component__WEBPACK_IMPORTED_MODULE_17__ = __webpack_require__(/*! ./logs.component */ "./src/app/views/diagnostics/logs.component.ts");
/* harmony import */ var _traces_component__WEBPACK_IMPORTED_MODULE_18__ = __webpack_require__(/*! ./traces.component */ "./src/app/views/diagnostics/traces.component.ts");
/* harmony import */ var _tracedetails_component__WEBPACK_IMPORTED_MODULE_19__ = __webpack_require__(/*! ./tracedetails.component */ "./src/app/views/diagnostics/tracedetails.component.ts");
/* harmony import */ var _status_component__WEBPACK_IMPORTED_MODULE_20__ = __webpack_require__(/*! ./status.component */ "./src/app/views/diagnostics/status.component.ts");
/* harmony import */ var _search_component__WEBPACK_IMPORTED_MODULE_21__ = __webpack_require__(/*! ./search.component */ "./src/app/views/diagnostics/search.component.ts");
var __decorate = (undefined && undefined.__decorate) || function (decorators, target, key, desc) {
    var c = arguments.length, r = c < 3 ? target : desc === null ? desc = Object.getOwnPropertyDescriptor(target, key) : desc, d;
    if (typeof Reflect === "object" && typeof Reflect.decorate === "function") r = Reflect.decorate(decorators, target, key, desc);
    else for (var i = decorators.length - 1; i >= 0; i--) if (d = decorators[i]) r = (c < 3 ? d(r) : c > 3 ? d(target, key, r) : d(target, key)) || r;
    return c > 3 && r && Object.defineProperty(target, key, r), r;
};
// Angular



// Ngx-bootstrap











// Components Routing

// Modal Component

// Charts

// Components





var DiagnosticsModule = /** @class */ (function () {
    function DiagnosticsModule() {
    }
    DiagnosticsModule = __decorate([
        Object(_angular_core__WEBPACK_IMPORTED_MODULE_2__["NgModule"])({
            imports: [
                _angular_common__WEBPACK_IMPORTED_MODULE_0__["CommonModule"],
                _angular_forms__WEBPACK_IMPORTED_MODULE_1__["FormsModule"],
                _diagnostics_routing_module__WEBPACK_IMPORTED_MODULE_14__["DiagnosticsRoutingModule"],
                ngx_bootstrap_dropdown__WEBPACK_IMPORTED_MODULE_6__["BsDropdownModule"].forRoot(),
                ngx_bootstrap_tabs__WEBPACK_IMPORTED_MODULE_3__["TabsModule"],
                ngx_bootstrap_carousel__WEBPACK_IMPORTED_MODULE_4__["CarouselModule"].forRoot(),
                ngx_bootstrap_collapse__WEBPACK_IMPORTED_MODULE_5__["CollapseModule"].forRoot(),
                ngx_bootstrap_pagination__WEBPACK_IMPORTED_MODULE_7__["PaginationModule"].forRoot(),
                ngx_bootstrap_popover__WEBPACK_IMPORTED_MODULE_8__["PopoverModule"].forRoot(),
                ngx_bootstrap_progressbar__WEBPACK_IMPORTED_MODULE_9__["ProgressbarModule"].forRoot(),
                ngx_bootstrap_tooltip__WEBPACK_IMPORTED_MODULE_10__["TooltipModule"].forRoot(),
                ngx_bootstrap_datepicker__WEBPACK_IMPORTED_MODULE_11__["BsDatepickerModule"].forRoot(),
                ngx_bootstrap_modal__WEBPACK_IMPORTED_MODULE_15__["ModalModule"].forRoot(),
                ng2_charts_ng2_charts__WEBPACK_IMPORTED_MODULE_16__["ChartsModule"],
                ngx_bootstrap_alert__WEBPACK_IMPORTED_MODULE_12__["AlertModule"].forRoot(),
                _nomadreservations_ngx_codemirror__WEBPACK_IMPORTED_MODULE_13__["CodemirrorModule"]
            ],
            declarations: [
                _logs_component__WEBPACK_IMPORTED_MODULE_17__["LogsComponent"],
                _traces_component__WEBPACK_IMPORTED_MODULE_18__["TracesComponent"],
                _tracedetails_component__WEBPACK_IMPORTED_MODULE_19__["TraceDetailsComponent"],
                _status_component__WEBPACK_IMPORTED_MODULE_20__["StatusComponent"],
                _search_component__WEBPACK_IMPORTED_MODULE_21__["SearchComponent"]
            ]
        })
    ], DiagnosticsModule);
    return DiagnosticsModule;
}());



/***/ }),

/***/ "./src/app/views/diagnostics/logs.component.html":
/*!*******************************************************!*\
  !*** ./src/app/views/diagnostics/logs.component.html ***!
  \*******************************************************/
/*! no static exports found */
/***/ (function(module, exports) {

module.exports = "<ol class=\"breadcrumb breadcrumb-body\">\r\n  <table>\r\n    <tr>\r\n      <td class=\"breadcrumb-icon\"></td>\r\n      <td class=\"breadcrumb-title\">\r\n        <strong>Logs</strong> -\r\n        <input type=\"text\" placeholder=\"Showing dates\" class=\"widget-date widget-range fa-pointer\" bsDaterangepicker [bsConfig]=\"bsConfig\"\r\n          [bsValue]=\"bsValue\" [(ngModel)]=\"bsValue\" (ngModelChange)=\"getApplications()\" />\r\n\r\n        <span class=\"fa-pointer float-right\" (click)=\"getApplications()\">\r\n            <i class=\"fa fa-refresh\"></i>&nbsp;\r\n            <span>Refresh</span>\r\n        </span>\r\n      </td>\r\n    </tr>\r\n  </table>\r\n</ol>\r\n\r\n<div>\r\n\r\n  <!-- Primer Row con Widgets -->\r\n  <div class=\"row\" [hidden]=\"!showChart\">\r\n\r\n    <div class=\"col-lg-9\">\r\n      <div class=\"card\">\r\n        <div class=\"card-body\">\r\n          <div class=\"chart-wrapper\">\r\n            <canvas baseChart class=\"chart\" [datasets]=\"mainChartData\" [labels]=\"mainChartLabels\" [options]=\"mainChartOptions\" [colors]=\"mainChartColours\"\r\n              [legend]=\"mainChartLegend\" [chartType]=\"mainChartType\"></canvas>\r\n          </div>\r\n        </div>\r\n      </div>\r\n    </div>\r\n\r\n    <!--/.col-->\r\n    <div class=\"col-lg-3 header\">\r\n      <div class=\"card\">\r\n        <div class=\"card-body p-0 clearfix\">\r\n          <i class=\"fa fa-bug bg-danger p-4 font-2xl mr-3 float-left\"></i>\r\n          <div class=\"h5 text-danger mb-0 pt-3\">{{errorCount}}</div>\r\n          <div class=\"text-muted text-uppercase font-weight-bold font-xs\">Errors</div>\r\n        </div>\r\n      </div>\r\n      <div class=\"card\">\r\n        <div class=\"card-body p-0 clearfix\">\r\n          <i class=\"fa fa-warning bg-warning p-4 font-2xl mr-3 float-left\"></i>\r\n          <div class=\"h5 text-warning mb-0 pt-3\">{{warningCount}}</div>\r\n          <div class=\"text-muted text-uppercase font-weight-bold font-xs\">Warnings</div>\r\n        </div>\r\n      </div>\r\n      <div class=\"card\">\r\n        <div class=\"card-body p-0 clearfix\">\r\n          <i class=\"fa fa-info bg-success p-4 font-2xl mr-3 float-left\"></i>\r\n          <div class=\"h5 text-info mb-0 pt-3\">{{statsCount}}</div>\r\n          <div class=\"text-muted text-uppercase font-weight-bold font-xs\">Stats</div>\r\n        </div>\r\n      </div>\r\n    </div>\r\n  </div>\r\n\r\n  <!-- Row con Charts -->\r\n\r\n\r\n  <!-- No Data Alert -->\r\n  <div class=\"card\" *ngIf=\"!showChart\">\r\n    <div class=\"card-body\">\r\n      <alert type=\"info\" class=\"no-bottom-margin-alert\">\r\n        <strong>Ups!, there is not data for the selected date range.</strong>\r\n        <br/> Please select another range of dates to get results.\r\n      </alert>\r\n    </div>\r\n  </div>\r\n\r\n  <!-- Tablas de Datos -->\r\n  <div *ngFor=\"let item of summary?.applications\">\r\n    <div class=\"row\">\r\n      <div class=\"col-lg-12\">\r\n        <div class=\"card\" [ngClass]=\"{\r\n            'card-accent-success': currentLogData(item).level === 'Stats',\r\n            'card-accent-danger': currentLogData(item).level === 'Error',\r\n            'card-accent-warning': currentLogData(item).level === 'Warning',\r\n            'card-accent-secondary': currentLogData(item).level !== 'Stats' && currentLogData(item).level !== 'Error' && currentLogData(item).level !== 'Warning',\r\n            'card-collapsed': currentLogData(item).isCollapsed,\r\n            'card-expanded': !currentLogData(item).isCollapsed\r\n          }\">\r\n          <div class=\"card-header\" [ngClass]=\"{ 'background-white': currentLogData(item).isCollapsed }\">\r\n            <table class=\"card-table\">\r\n              <tr>\r\n                <td class=\"td-icon\">\r\n                  <i class=\"fa fa-pointer\" [ngClass]=\"{\r\n                      'fa-caret-right': currentLogData(item).isCollapsed,\r\n                      'fa-caret-down': !currentLogData(item).isCollapsed }\" (click)=\"currentLogData(item).isCollapsed = !currentLogData(item).isCollapsed\"></i>\r\n                </td>\r\n                <td class=\"td-title\" (click)=\"currentLogData(item).isCollapsed = !currentLogData(item).isCollapsed\">{{item.application}}</td>\r\n                <td class=\"td-level\" >\r\n                  <span class=\"expandedSpan\" *ngIf=\"!currentLogData(item).isCollapsed\">\r\n                      <div class=\"fa-pointer\" (click)=\"goToPage(item, currentLogData(item).unwrappedData?.pageNumber)\">\r\n                        <i class=\"fa fa-refresh\"></i>\r\n                        {{currentLogData(item).unwrappedData?.totalResults}} items &nbsp;\r\n                      </div>\r\n                  </span>\r\n                  <div class=\"btn-group float-right right-buttons\" dropdown [isDisabled]=\"item.levels.length == 1\" [hidden]=\"currentLogData(item).isCollapsed\">\r\n                    <button dropdownToggle type=\"button\" class=\"btn dropdown-toggle\" [ngClass]=\"{\r\n                                  'btn-success': currentLogData(item).level === 'Stats',\r\n                                  'btn-danger': currentLogData(item).level === 'Error',\r\n                                  'btn-warning': currentLogData(item).level === 'Warning',\r\n                                  'btn-secondary': currentLogData(item).level !== 'Stats' && currentLogData(item).level !== 'Error' && currentLogData(item).level !== 'Warning'}\">\r\n                      {{currentLogData(item).level}}\r\n                      <span class=\"caret\"></span>\r\n                    </button>\r\n                    <ul *dropdownMenu class=\"dropdown-menu dropdown-menu-right\" role=\"menu\">\r\n                      <li role=\"menuitem\" *ngFor=\"let level of item.levels\">\r\n                        <a class=\"dropdown-item\" (click)=\"changeLevel(item, level.name)\">{{level.name}}</a>\r\n                      </li>\r\n                    </ul>\r\n                  </div>\r\n                  <span *ngIf=\"currentLogData(item).isCollapsed\">{{levelsLegend(item)}}</span>\r\n                </td>\r\n              </tr>\r\n            </table>\r\n          </div>\r\n          <div class=\"card-body\" [collapse]=\"currentLogData(item).isCollapsed\">\r\n            <table class=\"table table-sm table-striped table-log\">\r\n              <thead>\r\n                <tr>\r\n                  <th style=\"width: 135px\">Timestamp</th>\r\n                  <th style=\"width: max-content;\">Message</th>\r\n                  <th style=\"width: 100px\">Machine</th>\r\n                </tr>\r\n              </thead>\r\n              <tbody>\r\n                <tr *ngFor=\"let dataItem of currentLogData(item).unwrappedData?.data\">\r\n                  <td>{{dataItem.timestamp | date:'dd/MM/yy HH:mm:ss'}}</td>\r\n                  <td class=\"breakWord\">\r\n                    <span *ngIf=\"dataItem.type != null\">&lt;{{dataItem.type}}&gt;</span>\r\n                    {{dataItem.message}}\r\n                    <div class=\"logButtons\">\r\n                      <span *ngIf=\"dataItem.exception != null\" (click)=\"showException(dataItem)\" class=\"badge button-exception\">Exception</span>\r\n                      <span *ngIf=\"dataItem.group != null && dataItem.group != 'AspNetCore' && dataItem.group.length > 3\" class=\"badge button-group\"\r\n                      [routerLink]=\"['/diagnostics/search']\" [queryParams]=\"{\r\n                        term: dataItem.group,\r\n                        env: environmentName,\r\n                        fromDate: dataItem.timestamp,\r\n                        toDate: dataItem.timestamp\r\n                      }\">Transaction</span>\r\n                    </div>\r\n                  </td>\r\n                  <td>\r\n                    {{dataItem.machine}}\r\n                  </td>\r\n                </tr>\r\n              </tbody>\r\n            </table>\r\n          </div>\r\n          <div class=\"card-footer\" [collapse]=\"currentLogData(item).isCollapsed\" *ngIf=\"currentLogData(item).unwrappedData?.totalPages > 1\">\r\n            <ul class=\"pagination\">\r\n              <li class=\"page-item\">\r\n                <a class=\"page-link\" (click)=\"goToPage(item, currentLogData(item).unwrappedData?.pageNumber - 1)\">Prev</a>\r\n              </li>\r\n              <li *ngFor=\"let i of currentLogData(item).totalPagesArray\" class=\"page-item\" [ngClass]=\"{ 'active' : i == currentLogData(item).unwrappedData?.pageNumber }\">\r\n                <a class=\"page-link\" (click)=\"goToPage(item, i)\">{{i+1}}</a>\r\n              </li>\r\n              <li class=\"page-item\">\r\n                <a class=\"page-link\" (click)=\"goToPage(item, currentLogData(item).unwrappedData?.pageNumber + 1)\">Next</a>\r\n              </li>\r\n            </ul>\r\n          </div>\r\n        </div>\r\n      </div>\r\n    </div>\r\n  </div>\r\n\r\n  <div class=\"row\">&nbsp;</div>\r\n\r\n</div>\r\n\r\n<!-- Exception View -->\r\n<div bsModal #exceptionModal=\"bs-modal\" class=\"modal fade exception-view\" tabindex=\"-1\" role=\"dialog\" aria-labelledby=\"myModalLabel\" aria-hidden=\"true\">\r\n  <div class=\"modal-dialog modal-danger modal-exception\" role=\"document\">\r\n    <div class=\"modal-content\">\r\n      <div class=\"modal-header\">\r\n        <h4 class=\"modal-title\">Exception View</h4>\r\n        <button type=\"button\" class=\"close\" (click)=\"exceptionModal.hide()\" aria-label=\"Close\">\r\n          <span aria-hidden=\"true\">&times;</span>\r\n        </button>\r\n      </div>\r\n      <div class=\"modal-body\">\r\n\r\n        <div class=\"exceptionHeader\">\r\n          <div class=\"headerRow\">\r\n            <div>Timestamp</div>\r\n            <div>Application</div>\r\n            <div>Machine</div>\r\n          </div>\r\n          <div class=\"valueRow\">\r\n            <div>{{exceptionTimestamp | date:'dd/MM/yy HH:mm:ss.SSS'}}</div>\r\n            <div>{{exceptionApplication}}</div>\r\n            <div>{{exceptionMachine}}</div>\r\n          </div>\r\n        </div>\r\n        <div class=\"exceptionBody\">\r\n          <div class=\"item\" *ngIf=\"exceptionData?.exceptionType\">\r\n            <div class=\"key\">Type</div>\r\n            <div class=\"value\">{{exceptionData?.exceptionType}}</div>\r\n          </div>\r\n          <div class=\"item\" *ngIf=\"exceptionData?.source\">\r\n            <div class=\"key\">Source</div>\r\n            <div class=\"value\">{{exceptionData?.source}}</div>\r\n          </div>\r\n          <div class=\"item\">\r\n            <div class=\"key\">Message</div>\r\n            <div class=\"value important\">{{exceptionData?.message}}</div>\r\n          </div>\r\n          <div class=\"data\" *ngIf=\"exceptionData?.data && exceptionData.data.length > 0\">\r\n            <div class=\"item\" *ngFor=\"let itemData of exceptionData?.data\">\r\n              <div class=\"key\">{{itemData.key}}</div>\r\n              <div class=\"value important\">{{itemData.value}}</div>\r\n            </div>\r\n          </div>\r\n          <div class=\"exceptionStacktrace\">\r\n              <div class=\"title\">Stacktrace</div>\r\n              <div class=\"data\">\r\n                <div class=\"content\" *ngIf=\"exceptionData?.stackTrace\">{{exceptionData?.stackTrace}}</div>\r\n                <div class=\"content\" *ngIf=\"!(exceptionData?.stackTrace)\">Stacktrace data is not available</div>\r\n              </div>\r\n          </div>\r\n        </div>\r\n        <div class=\"exceptionBody inner\" *ngFor=\"let innerException of innerExceptionsData\">\r\n          <div class=\"item\" *ngIf=\"innerException?.exceptionType\">\r\n            <div class=\"key\">Type</div>\r\n            <div class=\"value\">{{innerException?.exceptionType}}</div>\r\n          </div>\r\n          <div class=\"item\" *ngIf=\"innerException?.source\">\r\n            <div class=\"key\">Source</div>\r\n            <div class=\"value\">{{innerException?.source}}</div>\r\n          </div>\r\n          <div class=\"item\">\r\n            <div class=\"key\">Message</div>\r\n            <div class=\"value important\">{{innerException?.message}}</div>\r\n          </div>\r\n          <div class=\"data\" *ngIf=\"innerException?.data && innerException.data.length > 0\">\r\n            <div class=\"item\" *ngFor=\"let itemData of innerException?.data\">\r\n              <div class=\"key\">{{itemData.key}}</div>\r\n              <div class=\"value important\">{{itemData.value}}</div>\r\n            </div>\r\n          </div>\r\n          <div class=\"exceptionStacktrace\">\r\n              <div class=\"title\">Stacktrace</div>\r\n              <div class=\"data\">\r\n                <div class=\"content\" *ngIf=\"innerException?.stackTrace\">{{innerException?.stackTrace}}</div>\r\n                <div class=\"content\" *ngIf=\"!(innerException?.stackTrace)\">Stacktrace data is not available</div>\r\n              </div>\r\n          </div>\r\n        </div>\r\n\r\n      </div>\r\n      <div class=\"modal-footer\">\r\n        <button type=\"button\" class=\"btn btn-secondary\" (click)=\"exceptionModal.hide()\">Close</button>\r\n      </div>\r\n    </div>\r\n    <!-- /.modal-content -->\r\n  </div>\r\n  <!-- /.modal-dialog -->\r\n</div>\r\n<!-- /.modal -->\r\n"

/***/ }),

/***/ "./src/app/views/diagnostics/logs.component.ts":
/*!*****************************************************!*\
  !*** ./src/app/views/diagnostics/logs.component.ts ***!
  \*****************************************************/
/*! exports provided: LogsComponent */
/***/ (function(module, __webpack_exports__, __webpack_require__) {

"use strict";
__webpack_require__.r(__webpack_exports__);
/* harmony export (binding) */ __webpack_require__.d(__webpack_exports__, "LogsComponent", function() { return LogsComponent; });
/* harmony import */ var _angular_router__WEBPACK_IMPORTED_MODULE_0__ = __webpack_require__(/*! @angular/router */ "./node_modules/@angular/router/fesm5/router.js");
/* harmony import */ var _angular_core__WEBPACK_IMPORTED_MODULE_1__ = __webpack_require__(/*! @angular/core */ "./node_modules/@angular/core/fesm5/core.js");
/* harmony import */ var _services_api_api_query_service__WEBPACK_IMPORTED_MODULE_2__ = __webpack_require__(/*! ../../services/api/api/query.service */ "./src/app/services/api/api/query.service.ts");
/* harmony import */ var _environments_environment__WEBPACK_IMPORTED_MODULE_3__ = __webpack_require__(/*! ../../../environments/environment */ "./src/environments/environment.ts");
/* harmony import */ var ngx_bootstrap_datepicker__WEBPACK_IMPORTED_MODULE_4__ = __webpack_require__(/*! ngx-bootstrap/datepicker */ "./node_modules/ngx-bootstrap/datepicker/index.js");
/* harmony import */ var ngx_bootstrap_chronos__WEBPACK_IMPORTED_MODULE_5__ = __webpack_require__(/*! ngx-bootstrap/chronos */ "./node_modules/ngx-bootstrap/chronos/index.js");
/* harmony import */ var ngx_bootstrap_locale__WEBPACK_IMPORTED_MODULE_6__ = __webpack_require__(/*! ngx-bootstrap/locale */ "./node_modules/ngx-bootstrap/locale.js");
/* harmony import */ var ngx_bootstrap_chronos_test_chain__WEBPACK_IMPORTED_MODULE_7__ = __webpack_require__(/*! ngx-bootstrap/chronos/test/chain */ "./node_modules/ngx-bootstrap/chronos/test/chain.js");
/* harmony import */ var _services_api_model_loglevel__WEBPACK_IMPORTED_MODULE_8__ = __webpack_require__(/*! ../../services/api/model/loglevel */ "./src/app/services/api/model/loglevel.ts");
/* harmony import */ var ngx_bootstrap_modal__WEBPACK_IMPORTED_MODULE_9__ = __webpack_require__(/*! ngx-bootstrap/modal */ "./node_modules/ngx-bootstrap/modal/index.js");
/* harmony import */ var ng2_charts_ng2_charts__WEBPACK_IMPORTED_MODULE_10__ = __webpack_require__(/*! ng2-charts/ng2-charts */ "./node_modules/ng2-charts/ng2-charts.js");
/* harmony import */ var ng2_charts_ng2_charts__WEBPACK_IMPORTED_MODULE_10___default = /*#__PURE__*/__webpack_require__.n(ng2_charts_ng2_charts__WEBPACK_IMPORTED_MODULE_10__);
/* harmony import */ var _coreui_coreui_dist_js_coreui_utilities__WEBPACK_IMPORTED_MODULE_11__ = __webpack_require__(/*! @coreui/coreui/dist/js/coreui-utilities */ "./node_modules/@coreui/coreui/dist/js/coreui-utilities.js");
/* harmony import */ var _coreui_coreui_dist_js_coreui_utilities__WEBPACK_IMPORTED_MODULE_11___default = /*#__PURE__*/__webpack_require__.n(_coreui_coreui_dist_js_coreui_utilities__WEBPACK_IMPORTED_MODULE_11__);
/* harmony import */ var _coreui_coreui_plugin_chartjs_custom_tooltips__WEBPACK_IMPORTED_MODULE_12__ = __webpack_require__(/*! @coreui/coreui-plugin-chartjs-custom-tooltips */ "./node_modules/@coreui/coreui-plugin-chartjs-custom-tooltips/dist/umd/custom-tooltips.js");
/* harmony import */ var _coreui_coreui_plugin_chartjs_custom_tooltips__WEBPACK_IMPORTED_MODULE_12___default = /*#__PURE__*/__webpack_require__.n(_coreui_coreui_plugin_chartjs_custom_tooltips__WEBPACK_IMPORTED_MODULE_12__);
var __decorate = (undefined && undefined.__decorate) || function (decorators, target, key, desc) {
    var c = arguments.length, r = c < 3 ? target : desc === null ? desc = Object.getOwnPropertyDescriptor(target, key) : desc, d;
    if (typeof Reflect === "object" && typeof Reflect.decorate === "function") r = Reflect.decorate(decorators, target, key, desc);
    else for (var i = decorators.length - 1; i >= 0; i--) if (d = decorators[i]) r = (c < 3 ? d(r) : c > 3 ? d(target, key, r) : d(target, key)) || r;
    return c > 3 && r && Object.defineProperty(target, key, r), r;
};
var __metadata = (undefined && undefined.__metadata) || function (k, v) {
    if (typeof Reflect === "object" && typeof Reflect.metadata === "function") return Reflect.metadata(k, v);
};







Object(ngx_bootstrap_chronos__WEBPACK_IMPORTED_MODULE_5__["defineLocale"])('en-gb', ngx_bootstrap_locale__WEBPACK_IMPORTED_MODULE_6__["enGbLocale"]);




// Charts


var LogsComponent = /** @class */ (function () {
    // Constructor
    function LogsComponent(_queryService, localeService, _activatedRoute, _router) {
        this._queryService = _queryService;
        this.localeService = localeService;
        this._activatedRoute = _activatedRoute;
        this._router = _router;
        this._defaultPageSize = 15;
        // Data Cache
        this.dataCache = {};
        this.mainChartData1 = [];
        this.mainChartData2 = [];
        this.mainChartData = [
            {
                data: this.mainChartData1,
                label: 'Error'
            },
            {
                data: this.mainChartData2,
                label: 'Warning'
            }
        ];
        this.mainChartColours = [
            {
                backgroundColor: Object(_coreui_coreui_dist_js_coreui_utilities__WEBPACK_IMPORTED_MODULE_11__["hexToRgba"])(Object(_coreui_coreui_dist_js_coreui_utilities__WEBPACK_IMPORTED_MODULE_11__["getStyle"])('--danger'), 100),
                borderColor: Object(_coreui_coreui_dist_js_coreui_utilities__WEBPACK_IMPORTED_MODULE_11__["getStyle"])('--danger'),
                pointHoverBackgroundColor: '#fff'
            },
            {
                backgroundColor: Object(_coreui_coreui_dist_js_coreui_utilities__WEBPACK_IMPORTED_MODULE_11__["hexToRgba"])(Object(_coreui_coreui_dist_js_coreui_utilities__WEBPACK_IMPORTED_MODULE_11__["getStyle"])('--warning'), 100),
                borderColor: Object(_coreui_coreui_dist_js_coreui_utilities__WEBPACK_IMPORTED_MODULE_11__["getStyle"])('--warning'),
                pointHoverBackgroundColor: '#fff',
            }
        ];
        this.mainChartLabels = [];
        this.mainChartOptions = {
            tooltips: {
                enabled: false,
                custom: _coreui_coreui_plugin_chartjs_custom_tooltips__WEBPACK_IMPORTED_MODULE_12__["CustomTooltips"],
                intersect: true,
                mode: 'index',
                position: 'nearest',
                callbacks: {
                    labelColor: function (tooltipItem, chart) {
                        return { backgroundColor: chart.data.datasets[tooltipItem.datasetIndex].borderColor };
                    }
                }
            },
            responsive: true,
            scaleShowVerticalLines: false,
            maintainAspectRatio: false,
            legend: {
                display: true
            }
        };
        this.mainChartLegend = false;
        this.mainChartType = 'bar';
        this.showChart = true;
    }
    // Public Methods
    LogsComponent.prototype.ngOnInit = function () {
        var initialDate = [Object(ngx_bootstrap_chronos_test_chain__WEBPACK_IMPORTED_MODULE_7__["moment"])().subtract(2, 'd').toDate(), Object(ngx_bootstrap_chronos_test_chain__WEBPACK_IMPORTED_MODULE_7__["moment"])().toDate()];
        this._queryParams = Object.assign({}, this._activatedRoute.snapshot.queryParams);
        if (this._queryParams.fromDate !== undefined) {
            initialDate[0] = ngx_bootstrap_chronos_test_chain__WEBPACK_IMPORTED_MODULE_7__["moment"].utc(this._queryParams.fromDate, 'YYYY-MM-DD').toDate();
        }
        if (this._queryParams.toDate !== undefined) {
            initialDate[1] = ngx_bootstrap_chronos_test_chain__WEBPACK_IMPORTED_MODULE_7__["moment"].utc(this._queryParams.toDate, 'YYYY-MM-DD').toDate();
        }
        this.bsConfig = Object.assign({}, {
            containerClass: 'theme-dark-blue',
            maxDate: Object(ngx_bootstrap_chronos_test_chain__WEBPACK_IMPORTED_MODULE_7__["moment"])().toDate(),
            showWeekNumbers: false
        });
        this.bsValue = initialDate;
        this.localeService.use('en-gb');
        this.getApplications();
    };
    LogsComponent.prototype.getApplications = function () {
        var _this = this;
        this.updateParams();
        this._queryService.apiQueryByEnvironmentLogsApplicationsGet(_environments_environment__WEBPACK_IMPORTED_MODULE_3__["environment"].name, this.bsValue[0], this.bsValue[1]).subscribe(function (x) {
            if (x === null) {
                return;
            }
            _this.dataCache = {};
            _this.summary = x;
            _this.errorCount = 0;
            _this.warningCount = 0;
            _this.statsCount = 0;
            _this.mainChartData1.length = 0;
            _this.mainChartData2.length = 0;
            _this.mainChartLabels.length = 0;
            _this.chart.chart.update();
            var series = {};
            var seriesArray = [];
            _this.showChart = false;
            x.levels.forEach(function (item) {
                if (item.name === _services_api_model_loglevel__WEBPACK_IMPORTED_MODULE_8__["LogLevelEnum"].Error) {
                    _this.errorCount = item.count;
                }
                if (item.name === _services_api_model_loglevel__WEBPACK_IMPORTED_MODULE_8__["LogLevelEnum"].Warning) {
                    _this.warningCount = item.count;
                }
                if (item.name === _services_api_model_loglevel__WEBPACK_IMPORTED_MODULE_8__["LogLevelEnum"].Stats) {
                    _this.statsCount = item.count;
                }
                item.series.forEach(function (value) {
                    var date = Object(ngx_bootstrap_chronos_test_chain__WEBPACK_IMPORTED_MODULE_7__["moment"])(value.date);
                    var valueDate = value.date.toString();
                    if (series[valueDate] === undefined) {
                        series[valueDate] = {};
                    }
                    series[valueDate]['Time'] = date.toDate().getTime();
                    series[valueDate]['Key'] = date.format('DD/MM/YYYY');
                    series[valueDate][item.name] = value.count;
                    _this.showChart = true;
                });
            });
            for (var key in series) {
                if (series.hasOwnProperty(key)) {
                    seriesArray.push(series[key]);
                }
            }
            seriesArray.sort(function (a, b) { return a.Time - b.Time; });
            for (var i = 0; i < seriesArray.length; i++) {
                var element = seriesArray[i];
                if (element[_services_api_model_loglevel__WEBPACK_IMPORTED_MODULE_8__["LogLevelEnum"].Error] !== undefined) {
                    _this.mainChartData1.push(element[_services_api_model_loglevel__WEBPACK_IMPORTED_MODULE_8__["LogLevelEnum"].Error]);
                }
                else {
                    _this.mainChartData1.push(0);
                }
                if (element[_services_api_model_loglevel__WEBPACK_IMPORTED_MODULE_8__["LogLevelEnum"].Warning] !== undefined) {
                    _this.mainChartData2.push(element[_services_api_model_loglevel__WEBPACK_IMPORTED_MODULE_8__["LogLevelEnum"].Warning]);
                }
                else {
                    _this.mainChartData2.push(0);
                }
                _this.mainChartLabels.push(element['Key']);
            }
            _this.chart.chart.update();
        });
    };
    LogsComponent.prototype.currentLogData = function (item) {
        var _this = this;
        var value = this.dataCache[item.application];
        if (value === undefined) {
            var newValue_1 = {
                environment: _environments_environment__WEBPACK_IMPORTED_MODULE_3__["environment"].name,
                level: item.levels[0].name,
                page: 0,
                pageSize: this._defaultPageSize,
                data: this._queryService.apiQueryByEnvironmentLogsByApplicationByLevelGet(_environments_environment__WEBPACK_IMPORTED_MODULE_3__["environment"].name, item.application, item.levels[0].name, this.bsValue[0], this.bsValue[1], 0, this._defaultPageSize),
                unwrappedData: null,
                totalPagesArray: [],
                isCollapsed: true
            };
            newValue_1.data.subscribe(function (x) { return _this.resolveSubscription(newValue_1, x); });
            this.dataCache[item.application] = newValue_1;
            return newValue_1;
        }
        if (value.data === null) {
            value.data = this._queryService.apiQueryByEnvironmentLogsByApplicationByLevelGet(_environments_environment__WEBPACK_IMPORTED_MODULE_3__["environment"].name, item.application, value.level, this.bsValue[0], this.bsValue[1], value.page, value.pageSize);
            value.data.subscribe(function (x) { return _this.resolveSubscription(value, x); });
        }
        return value;
    };
    LogsComponent.prototype.goToPage = function (item, page) {
        var _this = this;
        if (page === void 0) { page = 0; }
        if (page < 0) {
            return;
        }
        var value = this.currentLogData(item);
        if (page > value.totalPagesArray[value.totalPagesArray.length - 1]) {
            return;
        }
        value.page = page;
        value.data = this._queryService.apiQueryByEnvironmentLogsByApplicationByLevelGet(_environments_environment__WEBPACK_IMPORTED_MODULE_3__["environment"].name, item.application, value.level, this.bsValue[0], this.bsValue[1], value.page, value.pageSize);
        value.data.subscribe(function (x) { return _this.resolveSubscription(value, x); });
        return value;
    };
    LogsComponent.prototype.changeLevel = function (item, level) {
        var _this = this;
        var value = this.currentLogData(item);
        value.level = level;
        value.page = 0;
        value.data = this._queryService.apiQueryByEnvironmentLogsByApplicationByLevelGet(_environments_environment__WEBPACK_IMPORTED_MODULE_3__["environment"].name, item.application, value.level, this.bsValue[0], this.bsValue[1], value.page, value.pageSize);
        value.data.subscribe(function (x) { return _this.resolveSubscription(value, x); });
    };
    LogsComponent.prototype.changeRange = function (item) {
        var _this = this;
        var value = this.currentLogData(item);
        value.data = this._queryService.apiQueryByEnvironmentLogsByApplicationByLevelGet(_environments_environment__WEBPACK_IMPORTED_MODULE_3__["environment"].name, item.application, value.level, this.bsValue[0], this.bsValue[1], value.page, value.pageSize);
        value.data.subscribe(function (x) { return _this.resolveSubscription(value, x); });
    };
    LogsComponent.prototype.resolveSubscription = function (value, item) {
        var maxPages = 10;
        value.unwrappedData = item;
        if (item.totalPages < maxPages) {
            value.totalPagesArray = Array(item.totalPages).fill(0).map(function (a, i) { return i; });
        }
        else {
            var midPoint = maxPages / 2;
            if (item.pageNumber <= midPoint) {
                value.totalPagesArray = Array(maxPages).fill(0).map(function (a, i) { return i; });
            }
            else if (item.totalPages - item.pageNumber < midPoint) {
                var startPoint_1 = item.totalPages - maxPages;
                value.totalPagesArray = Array(maxPages).fill(0).map(function (a, i) { return startPoint_1 + i; });
            }
            else {
                var startPoint_2 = item.pageNumber - midPoint;
                value.totalPagesArray = Array(maxPages).fill(0).map(function (a, i) { return startPoint_2 + i; });
            }
        }
    };
    LogsComponent.prototype.showException = function (item) {
        this.exceptionTimestamp = item.timestamp;
        this.exceptionApplication = item.application;
        this.exceptionMachine = item.machine;
        this.exceptionData = item.exception;
        this.innerExceptionsData = [];
        if (this.exceptionData !== null) {
            this.createInnerExceptionData(this.exceptionData.innerException);
        }
        this.exceptionModal.show();
    };
    LogsComponent.prototype.createInnerExceptionData = function (item) {
        if (item === null) {
            return;
        }
        this.innerExceptionsData.push(item);
        this.createInnerExceptionData(item.innerException);
    };
    LogsComponent.prototype.levelsLegend = function (item) {
        if (item.levels === null) {
            return;
        }
        var textItems = [];
        item.levels.forEach(function (value) {
            var name = value.name;
            if (name !== 'Stats') {
                name += 's';
            }
            textItems.push(value.count + ' ' + name);
        });
        return textItems.join(', ');
    };
    // Private Methods
    LogsComponent.prototype.updateParams = function () {
        this._queryParams.env = _environments_environment__WEBPACK_IMPORTED_MODULE_3__["environment"].name;
        this._queryParams.fromDate = Object(ngx_bootstrap_chronos_test_chain__WEBPACK_IMPORTED_MODULE_7__["moment"])(this.bsValue[0]).format('YYYY-MM-DD');
        this._queryParams.toDate = Object(ngx_bootstrap_chronos_test_chain__WEBPACK_IMPORTED_MODULE_7__["moment"])(this.bsValue[1]).format('YYYY-MM-DD');
        this._router.navigate([], {
            relativeTo: this._activatedRoute,
            queryParams: this._queryParams,
            replaceUrl: true
        });
    };
    __decorate([
        Object(_angular_core__WEBPACK_IMPORTED_MODULE_1__["ViewChild"])('exceptionModal'),
        __metadata("design:type", ngx_bootstrap_modal__WEBPACK_IMPORTED_MODULE_9__["ModalDirective"])
    ], LogsComponent.prototype, "exceptionModal", void 0);
    __decorate([
        Object(_angular_core__WEBPACK_IMPORTED_MODULE_1__["ViewChild"])(ng2_charts_ng2_charts__WEBPACK_IMPORTED_MODULE_10__["BaseChartDirective"]),
        __metadata("design:type", ng2_charts_ng2_charts__WEBPACK_IMPORTED_MODULE_10__["BaseChartDirective"])
    ], LogsComponent.prototype, "chart", void 0);
    LogsComponent = __decorate([
        Object(_angular_core__WEBPACK_IMPORTED_MODULE_1__["Component"])({
            template: __webpack_require__(/*! ./logs.component.html */ "./src/app/views/diagnostics/logs.component.html")
        }),
        __metadata("design:paramtypes", [_services_api_api_query_service__WEBPACK_IMPORTED_MODULE_2__["QueryService"], ngx_bootstrap_datepicker__WEBPACK_IMPORTED_MODULE_4__["BsLocaleService"], _angular_router__WEBPACK_IMPORTED_MODULE_0__["ActivatedRoute"], _angular_router__WEBPACK_IMPORTED_MODULE_0__["Router"]])
    ], LogsComponent);
    return LogsComponent;
}());



/***/ }),

/***/ "./src/app/views/diagnostics/search.component.html":
/*!*********************************************************!*\
  !*** ./src/app/views/diagnostics/search.component.html ***!
  \*********************************************************/
/*! no static exports found */
/***/ (function(module, exports) {

module.exports = "<ol class=\"breadcrumb breadcrumb-body\">\r\n  <table>\r\n    <tr>\r\n      <td class=\"breadcrumb-icon\">\r\n      </td>\r\n      <td class=\"breadcrumb-title\">\r\n        <div class=\"input-group\">\r\n          <div class=\"input-group-prepend\">\r\n            <span class=\"input-group-text\">\r\n              <i class=\"fa fa-search\"></i>\r\n            </span>\r\n          </div>\r\n          <input type=\"text\" [(ngModel)]=\"searchValue\" (keyup.enter)=\"doSearch()\" id=\"search\" name=\"search\" class=\"form-control\" placeholder=\"Enter your search\">\r\n          <input type=\"text\" placeholder=\"Showing dates\" class=\"form-control fa-pointer input-date\" bsDaterangepicker [bsConfig]=\"bsConfig\"\r\n              [bsValue]=\"bsValue\" [(ngModel)]=\"bsValue\" (ngModelChange)=\"doSearch()\" />\r\n          <span class=\"input-group-append\">\r\n            <button class=\"btn btn-secondary\" type=\"button\" (click)=\"doSearch()\">Search</button>\r\n          </span>\r\n\r\n\r\n        </div>\r\n      </td>\r\n      <td class=\"breadcrumb-icon\">\r\n          <span class=\"fa-pointer float-right\" *ngIf=\"bHasResults == true\" (click)=\"getData()\">\r\n              <i class=\"fa fa-refresh\"></i>&nbsp;\r\n              <span>Refresh</span>\r\n          </span>\r\n      </td>\r\n    </tr>\r\n  </table>\r\n</ol>\r\n\r\n<div>\r\n\r\n  <div *ngIf=\"bProcessing\">\r\n    <strong>Searching ...</strong>\r\n  </div>\r\n\r\n  <div class=\"card\" *ngIf=\"bHasResults == false\">\r\n    <div class=\"card-body\">\r\n      <alert type=\"info\" class=\"no-bottom-margin-alert\">\r\n        <strong>Nothing found!</strong>\r\n        <br/> There are not results for this query, try another query and hit search again.\r\n      </alert>\r\n    </div>\r\n  </div>\r\n\r\n  <div *ngIf=\"bHasResults == true\">\r\n\r\n    <tabset>\r\n      <tab *ngFor=\"let groupItem of groupResults\" class=\"tab-search\">\r\n        <ng-template tabHeading><i class=\"icon-tag\"></i> Group: {{groupItem.groupName}} <span class=\"goToClass\" *ngIf=\"groupItem.groupName !== searchValue\" (click)=\"goToGroup(groupItem.groupName)\">Load Group</span></ng-template>\r\n\r\n        <div class=\"metadatas\" *ngIf=\"groupItem.metadata != null && groupItem.metadata.length > 0\">\r\n          <div class=\"item\" *ngFor=\"let metaItem of groupItem.metadata\">\r\n            <div class=\"itemKey\">{{metaItem.key}}</div>\r\n            <div class=\"itemValue\">{{metaItem.value}}</div>\r\n          </div>\r\n        </div>\r\n\r\n        <div class=\"card card-search\" *ngFor=\"let appItem of groupItem.items\" >\r\n          <div class=\"card-header\" (click)=\"appItem.hidden = !appItem.hidden\"  [ngStyle]=\"{ 'border-bottom' : appItem.hasError ? '4px solid #f86c6b' : appItem.hasWarning ? '4px solid #ffc421' : '' }\">\r\n            <div class=\"title\"><i class=\"fa fa-gear\" [ngStyle]=\"{ 'color' : appItem.hasError ? '#f14948' : appItem.hasWarning ? '#ffc107' : '#383838' }\"></i>&nbsp;&nbsp;{{appItem.appName}}</div>\r\n            <div class=\"counter\">{{appItem.items.length}} items</div>\r\n          </div>\r\n          <div class=\"card-body\" [collapse]=\"appItem.hidden\">\r\n            <div class=\"dvData\">\r\n              <div *ngFor=\"let rowItem of appItem.items; let isFirst = first\" class=\"dvRow\"  [ngClass]=\"{\r\n                'dv-level-error': rowItem.level == 'Error',\r\n                'dv-level-warning': rowItem.level == 'Warning',\r\n                'dv-level-success': rowItem.level == 'Stats',\r\n                'dv-level-trace' : rowItem.traceId != null,\r\n                'messageEnd' : rowItem.message && rowItem.message.indexOf('[END') > -1,\r\n                'messageStart' : rowItem.message && rowItem.message.indexOf('[START') > -1,\r\n                'nextIsStart' : rowItem.nextIsStart,\r\n                'prevIsEnd' : rowItem.prevIsEnd,\r\n                'noDiff' : !rowItem.diffTime\r\n              }\">\r\n                <div class=\"dvTimeCol\" *ngIf=\"!rowItem.diffTime\">\r\n                  {{rowItem.timestamp | date:'HH:mm:ss.SSS'}}\r\n                </div>\r\n                <div class=\"dvTimeCol addTime\" *ngIf=\"rowItem.diffTime\" [tooltip]=\"rowItem.timestamp | date:'HH:mm:ss.SSS'\">\r\n                  {{rowItem.diffTime}}\r\n                </div>\r\n                <div class=\"dvMessageCol\" *ngIf=\"rowItem.logId != null\">\r\n                    <div class=\"rightSide\">\r\n                      <span class=\"dvMessageType\" *ngIf=\"rowItem.type != null\">{{rowItem.type}}</span>\r\n                      <span *ngIf=\"rowItem.exception != null\" (click)=\"showException(rowItem)\" class=\"badge button-exception\">Show Exception</span>\r\n                    </div>\r\n                    <span class=\"spanMessage\">{{rowItem.message}}</span>\r\n                </div>\r\n                <div class=\"dvMessageCol\" *ngIf=\"rowItem.traceId != null\">\r\n                  <div class=\"dvText\"><!-- <i class=\"fa fa-file-code-o\" style=\"font-size:14px\"></i> -->{{rowItem.name}}</div>\r\n                  <div class=\"dvButtons\">\r\n                    <ng-container *ngFor=\"let tag of rowItem.tagsArray\">\r\n                        <button class=\"btn\" [ngClass]=\"{\r\n                          'button-info' : tag.key.indexOf('Status') == -1,\r\n                          'button-success' : tag.key.indexOf('Status') > -1 && tag.value == 'Success',\r\n                          'button-warning' : tag.key.indexOf('Status') > -1 && tag.value == 'Warning',\r\n                          'button-error' : tag.key.indexOf('Status') > -1 && tag.value == 'Error'\r\n                        }\" [tooltip]=\"tag.value\" [popover]=\"tag.value\" [popoverTitle]=\"tag.key\" placement=\"top\" [outsideClick]=\"true\">{{tag.key}}</button>\r\n                    </ng-container>\r\n                    <div class=\"separator\"></div>\r\n                    <button class=\"btn drop\" *ngIf=\"rowItem.hasXml\">\r\n                      XML\r\n                      <ul>\r\n                        <li (click)=\"showXmlData(rowItem.id, rowItem.name)\">View</li>\r\n                        <li (click)=\"downloadXmlData(rowItem.id, rowItem.name)\">Download</li>\r\n                      </ul>\r\n                    </button>\r\n                    <button class=\"btn drop\" *ngIf=\"rowItem.hasJson\">\r\n                      JSON\r\n                      <ul>\r\n                        <li (click)=\"showJsonData(rowItem.id, rowItem.name)\">View</li>\r\n                        <li (click)=\"downloadJsonData(rowItem.id, rowItem.name)\">Download</li>\r\n                      </ul>\r\n                    </button>\r\n                    <button class=\"btn drop\" *ngIf=\"rowItem.hasTxt\">\r\n                      TXT\r\n                      <ul>\r\n                        <li (click)=\"showTxtData(rowItem.id, rowItem.name)\">View</li>\r\n                        <li (click)=\"downloadTxtData(rowItem.id, rowItem.name)\">Download</li>\r\n                      </ul>\r\n                    </button>\r\n                  </div>\r\n                </div>\r\n              </div>\r\n            </div>\r\n          </div>\r\n        </div>\r\n\r\n      </tab>\r\n    </tabset>\r\n    <br/>\r\n\r\n  </div>\r\n\r\n</div>\r\n\r\n\r\n\r\n\r\n<!-- Exception View -->\r\n<div bsModal #exceptionModal=\"bs-modal\" class=\"modal fade exception-view\" tabindex=\"-1\" role=\"dialog\" aria-labelledby=\"myModalLabel\" aria-hidden=\"true\">\r\n  <div class=\"modal-dialog modal-danger modal-exception\" role=\"document\">\r\n    <div class=\"modal-content\">\r\n      <div class=\"modal-header\">\r\n        <h4 class=\"modal-title\">Exception View</h4>\r\n        <button type=\"button\" class=\"close\" (click)=\"exceptionModal.hide()\" aria-label=\"Close\">\r\n          <span aria-hidden=\"true\">&times;</span>\r\n        </button>\r\n      </div>\r\n      <div class=\"modal-body\">\r\n\r\n        <div class=\"exceptionHeader\">\r\n          <div class=\"headerRow\">\r\n            <div>Timestamp</div>\r\n            <div>Application</div>\r\n            <div>Machine</div>\r\n          </div>\r\n          <div class=\"valueRow\">\r\n            <div>{{exceptionTimestamp | date:'dd/MM/yy HH:mm:ss.SSS'}}</div>\r\n            <div>{{exceptionApplication}}</div>\r\n            <div>{{exceptionMachine}}</div>\r\n          </div>\r\n        </div>\r\n        <div class=\"exceptionBody\">\r\n          <div class=\"item\" *ngIf=\"exceptionData?.exceptionType\">\r\n            <div class=\"key\">Type</div>\r\n            <div class=\"value\">{{exceptionData?.exceptionType}}</div>\r\n          </div>\r\n          <div class=\"item\" *ngIf=\"exceptionData?.source\">\r\n            <div class=\"key\">Source</div>\r\n            <div class=\"value\">{{exceptionData?.source}}</div>\r\n          </div>\r\n          <div class=\"item\">\r\n            <div class=\"key\">Message</div>\r\n            <div class=\"value important\">{{exceptionData?.message}}</div>\r\n          </div>\r\n          <div class=\"data\" *ngIf=\"exceptionData?.data && exceptionData.data.length > 0\">\r\n            <div class=\"item\" *ngFor=\"let itemData of exceptionData?.data\">\r\n              <div class=\"key\">{{itemData.key}}</div>\r\n              <div class=\"value important\">{{itemData.value}}</div>\r\n            </div>\r\n          </div>\r\n          <div class=\"exceptionStacktrace\">\r\n              <div class=\"title\">Stacktrace</div>\r\n              <div class=\"data\">\r\n                <div class=\"content\" *ngIf=\"exceptionData?.stackTrace\">{{exceptionData?.stackTrace}}</div>\r\n                <div class=\"content\" *ngIf=\"!(exceptionData?.stackTrace)\">Stacktrace data is not available</div>\r\n              </div>\r\n          </div>\r\n        </div>\r\n        <div class=\"exceptionBody inner\" *ngFor=\"let innerException of innerExceptionsData\">\r\n          <div class=\"item\" *ngIf=\"innerException?.exceptionType\">\r\n            <div class=\"key\">Type</div>\r\n            <div class=\"value\">{{innerException?.exceptionType}}</div>\r\n          </div>\r\n          <div class=\"item\" *ngIf=\"innerException?.source\">\r\n            <div class=\"key\">Source</div>\r\n            <div class=\"value\">{{innerException?.source}}</div>\r\n          </div>\r\n          <div class=\"item\">\r\n            <div class=\"key\">Message</div>\r\n            <div class=\"value important\">{{innerException?.message}}</div>\r\n          </div>\r\n          <div class=\"data\" *ngIf=\"innerException?.data && innerException.data.length > 0\">\r\n            <div class=\"item\" *ngFor=\"let itemData of innerException?.data\">\r\n              <div class=\"key\">{{itemData.key}}</div>\r\n              <div class=\"value important\">{{itemData.value}}</div>\r\n            </div>\r\n          </div>\r\n          <div class=\"exceptionStacktrace\">\r\n              <div class=\"title\">Stacktrace</div>\r\n              <div class=\"data\">\r\n                <div class=\"content\" *ngIf=\"innerException?.stackTrace\">{{innerException?.stackTrace}}</div>\r\n                <div class=\"content\" *ngIf=\"!(innerException?.stackTrace)\">Stacktrace data is not available</div>\r\n              </div>\r\n          </div>\r\n        </div>\r\n\r\n      </div>\r\n      <div class=\"modal-footer\">\r\n        <button type=\"button\" class=\"btn btn-secondary\" (click)=\"exceptionModal.hide()\">Close</button>\r\n      </div>\r\n    </div>\r\n    <!-- /.modal-content -->\r\n  </div>\r\n  <!-- /.modal-dialog -->\r\n</div>\r\n<!-- /.modal -->\r\n\r\n\r\n<!-- Trace View -->\r\n<div bsModal #traceModal=\"bs-modal\" class=\"modal fade\" tabindex=\"-1\" role=\"dialog\" aria-labelledby=\"myModalLabel\" aria-hidden=\"true\">\r\n    <div class=\"modal-dialog modal-primary modal-code\" role=\"document\">\r\n      <div class=\"modal-content\">\r\n        <div class=\"modal-header\">\r\n          <h5 class=\"modal-title\">\r\n            <strong>Trace View: </strong> {{traceName}}</h5>\r\n          <button type=\"button\" class=\"close\" (click)=\"traceModal.hide()\" aria-label=\"Close\">\r\n            <span aria-hidden=\"true\">&times;</span>\r\n          </button>\r\n        </div>\r\n        <div class=\"modal-body\">\r\n          <ngx-codemirror></ngx-codemirror>\r\n        </div>\r\n      </div>\r\n    </div>\r\n  </div>\r\n"

/***/ }),

/***/ "./src/app/views/diagnostics/search.component.ts":
/*!*******************************************************!*\
  !*** ./src/app/views/diagnostics/search.component.ts ***!
  \*******************************************************/
/*! exports provided: SearchComponent */
/***/ (function(module, __webpack_exports__, __webpack_require__) {

"use strict";
__webpack_require__.r(__webpack_exports__);
/* harmony export (binding) */ __webpack_require__.d(__webpack_exports__, "SearchComponent", function() { return SearchComponent; });
/* harmony import */ var _angular_router__WEBPACK_IMPORTED_MODULE_0__ = __webpack_require__(/*! @angular/router */ "./node_modules/@angular/router/fesm5/router.js");
/* harmony import */ var _angular_core__WEBPACK_IMPORTED_MODULE_1__ = __webpack_require__(/*! @angular/core */ "./node_modules/@angular/core/fesm5/core.js");
/* harmony import */ var _services_api_api_query_service__WEBPACK_IMPORTED_MODULE_2__ = __webpack_require__(/*! ../../services/api/api/query.service */ "./src/app/services/api/api/query.service.ts");
/* harmony import */ var _environments_environment__WEBPACK_IMPORTED_MODULE_3__ = __webpack_require__(/*! ../../../environments/environment */ "./src/environments/environment.ts");
/* harmony import */ var ngx_bootstrap_chronos_test_chain__WEBPACK_IMPORTED_MODULE_4__ = __webpack_require__(/*! ngx-bootstrap/chronos/test/chain */ "./node_modules/ngx-bootstrap/chronos/test/chain.js");
/* harmony import */ var _services_api__WEBPACK_IMPORTED_MODULE_5__ = __webpack_require__(/*! ../../services/api */ "./src/app/services/api/index.ts");
/* harmony import */ var ngx_bootstrap_modal__WEBPACK_IMPORTED_MODULE_6__ = __webpack_require__(/*! ngx-bootstrap/modal */ "./node_modules/ngx-bootstrap/modal/index.js");
/* harmony import */ var _nomadreservations_ngx_codemirror__WEBPACK_IMPORTED_MODULE_7__ = __webpack_require__(/*! @nomadreservations/ngx-codemirror */ "./node_modules/@nomadreservations/ngx-codemirror/fesm5/nomadreservations-ngx-codemirror.js");
/* harmony import */ var ngx_bootstrap_datepicker__WEBPACK_IMPORTED_MODULE_8__ = __webpack_require__(/*! ngx-bootstrap/datepicker */ "./node_modules/ngx-bootstrap/datepicker/index.js");
/* harmony import */ var ngx_bootstrap_chronos__WEBPACK_IMPORTED_MODULE_9__ = __webpack_require__(/*! ngx-bootstrap/chronos */ "./node_modules/ngx-bootstrap/chronos/index.js");
/* harmony import */ var ngx_bootstrap_locale__WEBPACK_IMPORTED_MODULE_10__ = __webpack_require__(/*! ngx-bootstrap/locale */ "./node_modules/ngx-bootstrap/locale.js");
var __decorate = (undefined && undefined.__decorate) || function (decorators, target, key, desc) {
    var c = arguments.length, r = c < 3 ? target : desc === null ? desc = Object.getOwnPropertyDescriptor(target, key) : desc, d;
    if (typeof Reflect === "object" && typeof Reflect.decorate === "function") r = Reflect.decorate(decorators, target, key, desc);
    else for (var i = decorators.length - 1; i >= 0; i--) if (d = decorators[i]) r = (c < 3 ? d(r) : c > 3 ? d(target, key, r) : d(target, key)) || r;
    return c > 3 && r && Object.defineProperty(target, key, r), r;
};
var __metadata = (undefined && undefined.__metadata) || function (k, v) {
    if (typeof Reflect === "object" && typeof Reflect.metadata === "function") return Reflect.metadata(k, v);
};











Object(ngx_bootstrap_chronos__WEBPACK_IMPORTED_MODULE_9__["defineLocale"])('en-gb', ngx_bootstrap_locale__WEBPACK_IMPORTED_MODULE_10__["enGbLocale"]);
var SearchComponent = /** @class */ (function () {
    function SearchComponent(_queryService, _activatedRoute, _router, _codeMirror, localeService) {
        this._queryService = _queryService;
        this._activatedRoute = _activatedRoute;
        this._router = _router;
        this._codeMirror = _codeMirror;
        this.localeService = localeService;
        this.bProcessing = false;
        this.applications = [];
        this.logCollapsed = false;
    }
    // Public Methods
    SearchComponent.prototype.ngOnInit = function () {
        var initialDate = [Object(ngx_bootstrap_chronos_test_chain__WEBPACK_IMPORTED_MODULE_4__["moment"])().subtract(14, 'd').toDate(), Object(ngx_bootstrap_chronos_test_chain__WEBPACK_IMPORTED_MODULE_4__["moment"])().toDate()];
        this._queryParams = Object.assign({}, this._activatedRoute.snapshot.queryParams);
        if (this._queryParams.fromDate !== undefined) {
            initialDate[0] = ngx_bootstrap_chronos_test_chain__WEBPACK_IMPORTED_MODULE_4__["moment"].utc(this._queryParams.fromDate, 'YYYY-MM-DD').toDate();
        }
        if (this._queryParams.toDate !== undefined) {
            initialDate[1] = ngx_bootstrap_chronos_test_chain__WEBPACK_IMPORTED_MODULE_4__["moment"].utc(this._queryParams.toDate, 'YYYY-MM-DD').toDate();
        }
        this.bsConfig = Object.assign({}, {
            containerClass: 'theme-dark-blue',
            maxDate: Object(ngx_bootstrap_chronos_test_chain__WEBPACK_IMPORTED_MODULE_4__["moment"])().toDate(),
            showWeekNumbers: false
        });
        this.bsValue = initialDate;
        this.localeService.use('en-gb');
        if (this._queryParams.term !== undefined) {
            this.searchValue = this._queryParams.term;
            this.getData();
        }
    };
    SearchComponent.prototype.doSearch = function () {
        this.updateParams();
        this.getData();
    };
    SearchComponent.prototype.goToGroup = function (groupName) {
        this.searchValue = groupName;
        this.doSearch();
    };
    SearchComponent.prototype.getData = function () {
        var _this = this;
        if (this.searchValue === undefined || this.searchValue === null || this.searchValue.length === 0) {
            return;
        }
        this.bHasResults = null;
        this.bProcessing = true;
        var searchVal = this.searchValue;
        if (searchVal != null && (searchVal.startsWith('http://') || searchVal.startsWith('HTTP://'))) {
            var regex1 = /[0-9a-f]{8}-[0-9a-f]{4}-[1-5][0-9a-f]{3}-[89ab][0-9a-f]{3}-[0-9a-f]{12}/i;
            var regex2 = /[0-9a-f]{8}[0-9a-f]{4}[1-5][0-9a-f]{3}[89ab][0-9a-f]{3}[0-9a-f]{12}/i;
            var regex1Result = regex1.exec(searchVal);
            var regex2Result = regex2.exec(searchVal);
            var finalRes = [];
            if (regex1Result !== null) {
                finalRes = finalRes.concat(regex1Result);
            }
            if (regex2Result !== null) {
                finalRes = finalRes.concat(regex2Result);
            }
            searchVal = finalRes.join(' ');
        }
        this._queryService.apiQueryByEnvironmentSearchBySearchTermGet(_environments_environment__WEBPACK_IMPORTED_MODULE_3__["environment"].name, searchVal, this.bsValue[0], this.bsValue[1]).subscribe(function (data) {
            _this.bProcessing = false;
            if (data == null || data.data == null || data.data.length === 0) {
                _this.bHasResults = false;
                return;
            }
            _this.bHasResults = true;
            _this.searchResults = data;
            var groupArray = Array();
            if (_this.searchResults !== null) {
                if (_this.searchResults.data !== null) {
                    var _loop_1 = function (i) {
                        var dataItem = _this.searchResults.data[i];
                        var groupItem = groupArray.find(function (item) { return item.groupName === dataItem.group; });
                        if (groupItem === undefined) {
                            groupItem = {
                                groupName: dataItem.group,
                                items: [],
                                metadata: []
                            };
                            // Buscar los metadatas del grupo.
                            if (groupItem.groupName) {
                                _this._queryService.apiQueryGetGroupMetadata(_environments_environment__WEBPACK_IMPORTED_MODULE_3__["environment"].name, groupItem.groupName).subscribe(function (metadata) {
                                    if (metadata) {
                                        groupItem.metadata = metadata.filter(function (mitem) { return mitem.value !== null && mitem.value !== ''; });
                                    }
                                });
                            }
                            groupArray.push(groupItem);
                        }
                        var appItem = groupItem.items.find(function (item) { return item.appName === dataItem.application; });
                        if (appItem === undefined) {
                            appItem = {
                                appName: dataItem.application,
                                hidden: true,
                                hasError: false,
                                hasWarning: false,
                                items: []
                            };
                            groupItem.items.push(appItem);
                        }
                        if (dataItem.tags === null || dataItem.tags === undefined) {
                            dataItem.tags = '';
                        }
                        if (dataItem.traceId === undefined) {
                            dataItem.traceId = null;
                        }
                        if (dataItem.name === undefined) {
                            dataItem.name = null;
                        }
                        if (dataItem.type === undefined) {
                            dataItem.type = null;
                        }
                        if (dataItem.formats === undefined) {
                            dataItem.formats = null;
                        }
                        var itemTags = dataItem.tags.split(', ');
                        var tags = [];
                        for (var it = 0; it < itemTags.length; it++) {
                            var itemTagItem = itemTags[it].split(': ');
                            tags.push({ key: itemTagItem[0], value: itemTagItem[1] });
                        }
                        var nodeItem = {
                            assembly: dataItem.assembly,
                            code: dataItem.code,
                            exception: dataItem.exception,
                            id: dataItem.id,
                            application: dataItem.application,
                            instanceId: dataItem.instanceId,
                            level: dataItem.level,
                            logId: dataItem.logId,
                            machine: dataItem.machine,
                            message: dataItem.message,
                            timestamp: new Date(dataItem.timestamp),
                            type: dataItem.type,
                            tags: dataItem.tags,
                            tagsArray: tags,
                            name: dataItem.name,
                            traceId: dataItem.traceId,
                            nextIsStart: false,
                            prevIsEnd: false,
                            hasXml: ((dataItem.formats !== null && dataItem.formats.indexOf('XML') > -1) || dataItem.formats === null),
                            hasJson: ((dataItem.formats !== null && dataItem.formats.indexOf('JSON') > -1) || dataItem.formats === null),
                            hasTxt: (dataItem.formats !== null && dataItem.formats.indexOf('TXT') > -1)
                        };
                        if (nodeItem.tags.indexOf('Status: Error') > -1) {
                            appItem.hasError = true;
                        }
                        if (nodeItem.level === _services_api__WEBPACK_IMPORTED_MODULE_5__["NodeLogItem"].LevelEnum.Error) {
                            appItem.hasError = true;
                            // appItem.hidden = false;
                        }
                        if (nodeItem.level === _services_api__WEBPACK_IMPORTED_MODULE_5__["NodeLogItem"].LevelEnum.Warning) {
                            appItem.hasWarning = true;
                            // appItem.hidden = false;
                        }
                        appItem.items.push(nodeItem);
                    };
                    for (var i = 0; i < _this.searchResults.data.length; i++) {
                        _loop_1(i);
                    }
                }
            }
            for (var i = 0; i < groupArray.length; i++) {
                var groupItem = groupArray[i];
                for (var j = 0; j < groupItem.items.length; j++) {
                    var appItem = groupItem.items[j];
                    appItem.items.sort(function (a, b) {
                        var aNumber = a.timestamp.valueOf();
                        var bNumber = b.timestamp.valueOf();
                        if (aNumber === bNumber) {
                            if (a.logId !== null && a.message != null) {
                                if (b.logId !== null && b.message != null) {
                                    if (b.message.indexOf('[START') > -1) {
                                        return 1;
                                    }
                                    if (b.message.indexOf('[END') > -1) {
                                        return -1;
                                    }
                                    if (a.message.indexOf('[START') > -1) {
                                        return -1;
                                    }
                                    if (a.message.indexOf('[END') > -1) {
                                        return 1;
                                    }
                                }
                                if (b.traceId !== null) {
                                    if (a.message.indexOf('[END') > -1) {
                                        return 1;
                                    }
                                    if (a.message.indexOf('[START') > -1) {
                                        return 1;
                                    }
                                    return -1;
                                }
                            }
                            if (b.logId !== null && b.message != null) {
                                if (a.traceId !== null) {
                                    if (b.message.indexOf('[END') > -1) {
                                        return -1;
                                    }
                                    if (b.message.indexOf('[START') > -1) {
                                        return -1;
                                    }
                                    return 1;
                                }
                            }
                        }
                        if (a.timestamp < b.timestamp) {
                            return -1;
                        }
                        return 1;
                    });
                    var startIndex = [];
                    for (var n = 0; n < appItem.items.length; n++) {
                        var nodeItem = appItem.items[n];
                        var isStart = nodeItem.message && nodeItem.message.indexOf('[START') > -1;
                        var isEnd = nodeItem.message && nodeItem.message.indexOf('[END') > -1;
                        if (isStart) {
                            startIndex.push(n);
                        }
                        var started = startIndex.length > 0;
                        var startedIndex = started ? startIndex[startIndex.length - 1] : -1;
                        if (n > 0) {
                            if (started && startedIndex !== n) {
                                var oldNodeItem = appItem.items[startedIndex];
                                var duration = Object(ngx_bootstrap_chronos_test_chain__WEBPACK_IMPORTED_MODULE_4__["moment"])(nodeItem.timestamp).diff(oldNodeItem.timestamp);
                                var minutes = Math.floor(duration / 1000 / 60);
                                var seconds = Math.floor((duration / 1000) - (minutes * 60));
                                var milliseconds = duration - (Math.floor(duration / 1000) * 1000);
                                var diffTime = '+ ';
                                if (minutes > 0) {
                                    diffTime += minutes + 'min';
                                    if (seconds > 0 || milliseconds > 0) {
                                        diffTime += ', ';
                                    }
                                }
                                if (seconds > 0) {
                                    diffTime += seconds + 's';
                                    if (milliseconds > 0) {
                                        diffTime += ', ';
                                    }
                                }
                                diffTime += milliseconds + 'ms';
                                nodeItem.diffTime = diffTime;
                                // console.log(nodeItem.diffTime);
                            }
                            var prevItem = appItem.items[n - 1];
                            if (prevItem) {
                                var prevDuration = Object(ngx_bootstrap_chronos_test_chain__WEBPACK_IMPORTED_MODULE_4__["moment"])(nodeItem.timestamp).diff(prevItem.timestamp);
                                if (prevDuration > 5000 && !started) {
                                    nodeItem.prevIsEnd = true;
                                }
                            }
                        }
                        if (isEnd) {
                            startIndex.pop();
                        }
                    }
                }
            }
            _this.groupResults = groupArray;
            console.log(groupArray);
        });
    };
    SearchComponent.prototype.showException = function (item) {
        this.exceptionTimestamp = item.timestamp;
        this.exceptionApplication = item.application;
        this.exceptionMachine = item.machine;
        this.exceptionData = item.exception;
        this.innerExceptionsData = [];
        if (this.exceptionData !== null) {
            this.createInnerExceptionData(this.exceptionData.innerException);
        }
        this.exceptionModal.show();
    };
    SearchComponent.prototype.createInnerExceptionData = function (item) {
        if (item === null) {
            return;
        }
        this.innerExceptionsData.push(item);
        this.createInnerExceptionData(item.innerException);
    };
    SearchComponent.prototype.showXmlData = function (id, name) {
        var _this = this;
        this.traceName = name;
        this._queryService.apiQueryByEnvironmentTracesXmlByIdGet(_environments_environment__WEBPACK_IMPORTED_MODULE_3__["environment"].name, id).subscribe(function (x) {
            if (x) {
                _this.traceModal.show();
                _this.traceObject = x;
                _this._codeMirror.instance$.subscribe(function (editor) {
                    editor.setOption('mode', 'application/xml');
                    if (x.startsWith('{') || x.startsWith('[')) {
                        editor.setOption('mode', 'application/json');
                    }
                    editor.setOption('theme', 'material');
                    editor.setOption('readOnly', true);
                    editor.setOption('lineNumbers', true);
                    editor.setOption('matchBrackets', true);
                    editor.setOption('foldGutter', true);
                    editor.setOption('gutters', ['CodeMirror-linenumbers', 'CodeMirror-foldgutter']);
                    editor.setOption('extraKeys', {
                        'Ctrl-F': 'findPersistent'
                    });
                    editor.setValue(_this.traceObject);
                    editor.getDoc().setCursor({ line: 0, ch: 0 });
                    editor.getDoc().setSelection({ line: 0, ch: 0 }, { line: 0, ch: 0 }, { scroll: true });
                    editor.scrollTo(0, 0);
                    setTimeout(function () { return editor.refresh(); }, 200);
                });
            }
        });
    };
    SearchComponent.prototype.showJsonData = function (id, name) {
        var _this = this;
        this.traceName = name;
        this._queryService.apiQueryByEnvironmentTracesJsonByIdGet(_environments_environment__WEBPACK_IMPORTED_MODULE_3__["environment"].name, id).subscribe(function (x) {
            if (x) {
                _this.traceModal.show();
                _this.traceObject = x;
                _this._codeMirror.instance$.subscribe(function (editor) {
                    editor.setOption('mode', 'application/json');
                    if (x.startsWith('<')) {
                        editor.setOption('mode', 'application/xml');
                    }
                    editor.setOption('theme', 'material');
                    editor.setOption('readOnly', true);
                    editor.setOption('lineNumbers', true);
                    editor.setOption('matchBrackets', true);
                    editor.setOption('foldGutter', true);
                    editor.setOption('gutters', ['CodeMirror-linenumbers', 'CodeMirror-foldgutter']);
                    editor.setOption('extraKeys', {
                        'Ctrl-F': 'findPersistent'
                    });
                    editor.setValue(_this.traceObject);
                    editor.getDoc().setCursor({ line: 0, ch: 0 });
                    editor.getDoc().setSelection({ line: 0, ch: 0 }, { line: 0, ch: 0 }, { scroll: true });
                    editor.scrollTo(0, 0);
                    setTimeout(function () { return editor.refresh(); }, 200);
                });
            }
        });
    };
    SearchComponent.prototype.showTxtData = function (id, name) {
        var _this = this;
        this.traceName = name;
        this._queryService.apiQueryByEnvironmentTracesTxtByIdGet(_environments_environment__WEBPACK_IMPORTED_MODULE_3__["environment"].name, id).subscribe(function (x) {
            if (x) {
                _this.traceModal.show();
                _this.traceObject = x;
                _this._codeMirror.instance$.subscribe(function (editor) {
                    editor.setOption('mode', 'text/plain');
                    if (x.startsWith('<')) {
                        editor.setOption('mode', 'application/xml');
                    }
                    if (x.startsWith('{') || x.startsWith('[')) {
                        editor.setOption('mode', 'application/json');
                    }
                    editor.setOption('theme', 'material');
                    editor.setOption('readOnly', true);
                    editor.setOption('lineNumbers', true);
                    editor.setOption('matchBrackets', true);
                    editor.setOption('foldGutter', true);
                    editor.setOption('gutters', ['CodeMirror-linenumbers', 'CodeMirror-foldgutter']);
                    editor.setOption('extraKeys', {
                        'Ctrl-F': 'findPersistent'
                    });
                    editor.setValue(_this.traceObject);
                    editor.getDoc().setCursor({ line: 0, ch: 0 });
                    editor.getDoc().setSelection({ line: 0, ch: 0 }, { line: 0, ch: 0 }, { scroll: true });
                    editor.scrollTo(0, 0);
                    setTimeout(function () { return editor.refresh(); }, 200);
                });
            }
        });
    };
    SearchComponent.prototype.downloadXmlData = function (id, name) {
        var _this = this;
        this._queryService.apiQueryByEnvironmentTracesXmlByIdGet(_environments_environment__WEBPACK_IMPORTED_MODULE_3__["environment"].name, id).subscribe(function (data) {
            if (data) {
                _this.downloadFile(data, name + '.xml', 'application/xml');
            }
        });
    };
    SearchComponent.prototype.downloadJsonData = function (id, name) {
        var _this = this;
        this._queryService.apiQueryByEnvironmentTracesJsonByIdGet(_environments_environment__WEBPACK_IMPORTED_MODULE_3__["environment"].name, id).subscribe(function (data) {
            if (data) {
                _this.downloadFile(data, name + '.json', 'application/json');
            }
        });
    };
    SearchComponent.prototype.downloadTxtData = function (id, name) {
        var _this = this;
        this._queryService.apiQueryByEnvironmentTracesTxtByIdGet(_environments_environment__WEBPACK_IMPORTED_MODULE_3__["environment"].name, id).subscribe(function (data) {
            if (data) {
                _this.downloadFile(data, name + '.txt');
            }
        });
    };
    // Private Methods
    SearchComponent.prototype.updateParams = function () {
        this._queryParams.env = _environments_environment__WEBPACK_IMPORTED_MODULE_3__["environment"].name;
        this._queryParams.term = this.searchValue;
        this._queryParams.fromDate = Object(ngx_bootstrap_chronos_test_chain__WEBPACK_IMPORTED_MODULE_4__["moment"])(this.bsValue[0]).format('YYYY-MM-DD');
        this._queryParams.toDate = Object(ngx_bootstrap_chronos_test_chain__WEBPACK_IMPORTED_MODULE_4__["moment"])(this.bsValue[1]).format('YYYY-MM-DD');
        this._router.navigate([], {
            relativeTo: this._activatedRoute,
            queryParams: this._queryParams,
            replaceUrl: true
        });
    };
    //Download methods
    SearchComponent.prototype.downloadFile = function (data, fileName, type) {
        if (type === void 0) { type = "text/plain"; }
        // Create an invisible A element
        var a = document.createElement("a");
        a.style.display = "none";
        document.body.appendChild(a);
        // Set the HREF to a Blob representation of the data to be downloaded
        a.href = window.URL.createObjectURL(new Blob([data], { type: type }));
        // Use download attribute to set set desired file name
        a.setAttribute("download", fileName);
        // Trigger the download by simulating click
        a.click();
        // Cleanup
        window.URL.revokeObjectURL(a.href);
        document.body.removeChild(a);
    };
    __decorate([
        Object(_angular_core__WEBPACK_IMPORTED_MODULE_1__["ViewChild"])('exceptionModal'),
        __metadata("design:type", ngx_bootstrap_modal__WEBPACK_IMPORTED_MODULE_6__["ModalDirective"])
    ], SearchComponent.prototype, "exceptionModal", void 0);
    __decorate([
        Object(_angular_core__WEBPACK_IMPORTED_MODULE_1__["ViewChild"])('traceModal'),
        __metadata("design:type", ngx_bootstrap_modal__WEBPACK_IMPORTED_MODULE_6__["ModalDirective"])
    ], SearchComponent.prototype, "traceModal", void 0);
    SearchComponent = __decorate([
        Object(_angular_core__WEBPACK_IMPORTED_MODULE_1__["Component"])({
            template: __webpack_require__(/*! ./search.component.html */ "./src/app/views/diagnostics/search.component.html")
        }),
        __metadata("design:paramtypes", [_services_api_api_query_service__WEBPACK_IMPORTED_MODULE_2__["QueryService"],
            _angular_router__WEBPACK_IMPORTED_MODULE_0__["ActivatedRoute"],
            _angular_router__WEBPACK_IMPORTED_MODULE_0__["Router"],
            _nomadreservations_ngx_codemirror__WEBPACK_IMPORTED_MODULE_7__["CodemirrorService"],
            ngx_bootstrap_datepicker__WEBPACK_IMPORTED_MODULE_8__["BsLocaleService"]])
    ], SearchComponent);
    return SearchComponent;
}());



/***/ }),

/***/ "./src/app/views/diagnostics/status.component.html":
/*!*********************************************************!*\
  !*** ./src/app/views/diagnostics/status.component.html ***!
  \*********************************************************/
/*! no static exports found */
/***/ (function(module, exports) {

module.exports = "<div class=\"counter-page\">\r\n\r\n    <!-- No Data Alert -->\r\n    <div class=\"card noData\" *ngIf=\"noData == true\">\r\n      <div class=\"card-body\">\r\n        <alert type=\"info\" class=\"no-bottom-margin-alert\">\r\n          <strong>Ups!, there is not satus data in this environment</strong>\r\n        </alert>\r\n      </div>\r\n    </div>\r\n\r\n    <div class=\"countersViewport\" *ngIf=\"noData == false\">\r\n\r\n      <div class=\"countersSidebar little\" [ngClass]=\"{'hide': showSideBar}\" (click)=\"toggleSidebar()\">\r\n          <i class=\"fa fa-area-chart\"></i>\r\n      </div>\r\n      <div class=\"countersSidebar\" [ngClass]=\"{'hide': !showSideBar}\">\r\n        <span class=\"countersTitle\" (click)=\"hideSidebar()\">Counters</span>\r\n        <div class=\"appCounter\" *ngFor=\"let appItem of counters\">\r\n          <span (click)=\"toggleVisible(appItem)\"><i class=\"fa fa-gear\"></i>{{appItem.applicationName}}</span>\r\n          <div class=\"kindCounter\" [ngClass]=\"{'hide': !appItem.itemsVisible}\" *ngFor=\"let kindItem of appItem.items\">\r\n            <span (click)=\"toggleVisible(kindItem)\"><i class=\"fa fa-caret-right\"></i>{{kindItem.kindName}}</span>\r\n            <div class=\"categoryCounter\" [ngClass]=\"{'hide': !kindItem.itemsVisible}\" *ngFor=\"let categoryItem of kindItem.items\">\r\n              <span (click)=\"toggleVisible(categoryItem)\"><i class=\"fa fa-caret-right\"></i>{{categoryItem.categoryName}}</span>\r\n              <div class=\"counterItem\" [ngClass]=\"{'hide': !categoryItem.itemsVisible}\" *ngFor=\"let counterItem of categoryItem.items\">\r\n                <span (click)=\"toggleSelect(counterItem)\"><i class=\"fa\" [ngClass]=\"{'fa-square-o': !counterItem.selected, 'fa-square': counterItem.selected }\"></i>{{counterItem.name}}</span>\r\n              </div>\r\n            </div>\r\n          </div>\r\n        </div>\r\n      </div>\r\n\r\n      <div class=\"title\" (click)=\"hideSidebar()\">Services Status</div>\r\n\r\n      <div class=\"countersContent\" (click)=\"hideSidebar()\">\r\n\r\n        <div class=\"card counter-card\" *ngFor=\"let counterItem of shownCounters\">\r\n          <div class=\"card-header\">\r\n            <div class=\"app-title\">{{counterItem.application}}</div>\r\n            <div class=\"name-title\">{{counterItem.name}}</div>\r\n          </div>\r\n          <div class=\"card-body counter-body\">\r\n            <div class=\"chart-wrapper\">\r\n              <canvas baseChart class=\"chart\" style=\"height:250px; max-height: 250px;\"\r\n                [datasets]=\"counterItem.barChartData\"\r\n                [labels]=\"counterItem.barChartLabels\"\r\n                [options]=\"barChartOptions\"\r\n                [legend]=\"barChartLegend\"\r\n                [chartType]=\"barChartType\"\r\n                (chartHover)=\"chartHovered($event)\"\r\n                (chartClick)=\"chartClicked($event)\"></canvas>\r\n            </div>\r\n            <div>Values</div>\r\n          </div>\r\n        </div>\r\n\r\n      </div>\r\n\r\n    </div>\r\n\r\n</div>\r\n"

/***/ }),

/***/ "./src/app/views/diagnostics/status.component.ts":
/*!*******************************************************!*\
  !*** ./src/app/views/diagnostics/status.component.ts ***!
  \*******************************************************/
/*! exports provided: StatusComponent */
/***/ (function(module, __webpack_exports__, __webpack_require__) {

"use strict";
__webpack_require__.r(__webpack_exports__);
/* harmony export (binding) */ __webpack_require__.d(__webpack_exports__, "StatusComponent", function() { return StatusComponent; });
/* harmony import */ var _angular_router__WEBPACK_IMPORTED_MODULE_0__ = __webpack_require__(/*! @angular/router */ "./node_modules/@angular/router/fesm5/router.js");
/* harmony import */ var _angular_core__WEBPACK_IMPORTED_MODULE_1__ = __webpack_require__(/*! @angular/core */ "./node_modules/@angular/core/fesm5/core.js");
/* harmony import */ var _services_api_api_query_service__WEBPACK_IMPORTED_MODULE_2__ = __webpack_require__(/*! ../../services/api/api/query.service */ "./src/app/services/api/api/query.service.ts");
/* harmony import */ var _environments_environment__WEBPACK_IMPORTED_MODULE_3__ = __webpack_require__(/*! ../../../environments/environment */ "./src/environments/environment.ts");
/* harmony import */ var ngx_bootstrap_chronos_test_chain__WEBPACK_IMPORTED_MODULE_4__ = __webpack_require__(/*! ngx-bootstrap/chronos/test/chain */ "./node_modules/ngx-bootstrap/chronos/test/chain.js");
/* harmony import */ var ngx_bootstrap_chronos__WEBPACK_IMPORTED_MODULE_5__ = __webpack_require__(/*! ngx-bootstrap/chronos */ "./node_modules/ngx-bootstrap/chronos/index.js");
/* harmony import */ var ngx_bootstrap_locale__WEBPACK_IMPORTED_MODULE_6__ = __webpack_require__(/*! ngx-bootstrap/locale */ "./node_modules/ngx-bootstrap/locale.js");
var __decorate = (undefined && undefined.__decorate) || function (decorators, target, key, desc) {
    var c = arguments.length, r = c < 3 ? target : desc === null ? desc = Object.getOwnPropertyDescriptor(target, key) : desc, d;
    if (typeof Reflect === "object" && typeof Reflect.decorate === "function") r = Reflect.decorate(decorators, target, key, desc);
    else for (var i = decorators.length - 1; i >= 0; i--) if (d = decorators[i]) r = (c < 3 ? d(r) : c > 3 ? d(target, key, r) : d(target, key)) || r;
    return c > 3 && r && Object.defineProperty(target, key, r), r;
};
var __metadata = (undefined && undefined.__metadata) || function (k, v) {
    if (typeof Reflect === "object" && typeof Reflect.metadata === "function") return Reflect.metadata(k, v);
};







Object(ngx_bootstrap_chronos__WEBPACK_IMPORTED_MODULE_5__["defineLocale"])('en-gb', ngx_bootstrap_locale__WEBPACK_IMPORTED_MODULE_6__["enGbLocale"]);
var StatusComponent = /** @class */ (function () {
    // public barChartLabels: string[] = ['2006', '2007', '2008', '2009', '2010', '2011', '2012'];
    // public barChartData: any[] = [
    //   {data: [65, 59, 80, 81, 56, 55, 40], label: 'Series A'},
    //   {data: [28, 48, 40, 19, 86, 27, 90], label: 'Series B'}
    // ];
    function StatusComponent(_queryService, _activatedRoute, _router) {
        this._queryService = _queryService;
        this._activatedRoute = _activatedRoute;
        this._router = _router;
        this.selectedCounters = [];
        this.shownCounters = [];
        this.showSideBar = false;
        // barChart
        this.barChartOptions = {
            scaleShowVerticalLines: false,
            responsive: true,
            animation: false
        };
        this.barChartType = 'bar';
        this.barChartLegend = false;
    }
    StatusComponent.prototype.ngOnInit = function () {
        this._params = Object.assign({}, this._activatedRoute.snapshot.params);
        this._queryParams = Object.assign({}, this._activatedRoute.snapshot.queryParams);
        this.updateParams();
        this.getData();
    };
    StatusComponent.prototype.getData = function () {
        var _this = this;
        if (!_environments_environment__WEBPACK_IMPORTED_MODULE_3__["environment"].name) {
            return;
        }
        console.log(_environments_environment__WEBPACK_IMPORTED_MODULE_3__["environment"].name);
        this.noData = null;
        this.counters = null;
        this._queryService.getCounters(_environments_environment__WEBPACK_IMPORTED_MODULE_3__["environment"].name).subscribe(function (data) {
            if (data && data.length > 0) {
                _this.noData = false;
                _this.counters = _this.createCountersTree(data);
                _this.rawCounters = {};
                for (var i = 0; i < data.length; i++) {
                    var item = Object.assign(data[i], { selected: false, lastData: [], barChartLabels: [], barChartData: [] });
                    for (var j = 0; j < _this.selectedCounters.length; j++) {
                        if (_this.selectedCounters[i] === item.countersId) {
                            item.selected = true;
                        }
                    }
                    _this.rawCounters[data[i].countersId] = item;
                }
            }
            else {
                _this.noData = true;
            }
            console.log(_this.counters);
            console.log(_this.rawCounters);
        });
    };
    StatusComponent.prototype.refreshGraphs = function () {
        var _this = this;
        if (this.timerValue) {
            clearTimeout(this.timerValue);
        }
        var yesterdayTime = new Date().getTime() - (24 * 60 * 60 * 1000);
        var fromTime = new Date();
        fromTime.setTime(yesterdayTime);
        var _loop_1 = function (i) {
            var item = this_1.rawCounters[this_1.selectedCounters[i]];
            if (item !== undefined) {
                var lastTime = null;
                if (item.lastData !== null && item.lastData !== undefined && item.lastData.length > 0) {
                    lastTime = item.lastData[item.lastData.length - 1].timestamp;
                }
                this_1._queryService.getLastCounterValues(item.countersId, 'Week', _environments_environment__WEBPACK_IMPORTED_MODULE_3__["environment"].name, null, lastTime).subscribe(function (data) {
                    if (data) {
                        if (item.lastData !== null && item.lastData !== undefined && item.lastData.length > 0) {
                            // console.log(data);
                            item.lastData = item.lastData.slice(data.length - 1, item.lastData.length - data.length);
                            item.barChartLabels = item.barChartLabels.slice(data.length - 1, item.barChartLabels.length - data.length);
                            item.barChartData[0].data = item.barChartData[0].data.slice(data.length - 1, item.barChartData[0].data.length - data.length);
                            data.forEach(function (element) { return item.lastData.push(element); });
                            for (var j = 0; j < data.length; j++) {
                                var itemData = data[j];
                                item.barChartLabels.push(Object(ngx_bootstrap_chronos_test_chain__WEBPACK_IMPORTED_MODULE_4__["moment"])(itemData.timestamp).format('MM-DD (HH:mm)'));
                                item.barChartData[0].data.push(itemData.value);
                            }
                        }
                        else {
                            item.lastData = data;
                            item.barChartLabels = [];
                            item.barChartData = [{ data: [], label: 'Values' }];
                            for (var j = 0; j < data.length; j++) {
                                var itemData = data[j];
                                item.barChartLabels.push(Object(ngx_bootstrap_chronos_test_chain__WEBPACK_IMPORTED_MODULE_4__["moment"])(itemData.timestamp).format('MM-DD (HH:mm)'));
                                item.barChartData[0].data.push(itemData.value);
                            }
                        }
                        if (_this.shownCounters.findIndex(function (citem) { return citem.countersId === item.countersId; }) > -1) {
                            return;
                        }
                        _this.shownCounters.push(item);
                        _this.shownCounters.sort(function (a, b) {
                            if (a.application < b.application) {
                                return -1;
                            }
                            else if (a.application > b.application) {
                                return 1;
                            }
                            else {
                                if (a.kind < b.kind) {
                                    return -1;
                                }
                                else if (a.kind > b.kind) {
                                    return 1;
                                }
                                else {
                                    if (a.category < b.category) {
                                        return -1;
                                    }
                                    else if (a.category > b.category) {
                                        return 1;
                                    }
                                    else {
                                        if (a.name < b.name) {
                                            return -1;
                                        }
                                        else if (a.name > b.name) {
                                            return 1;
                                        }
                                        else {
                                            return 0;
                                        }
                                    }
                                }
                            }
                        });
                    }
                    console.log(item);
                });
            }
        };
        var this_1 = this;
        // console.log(fromTime);
        for (var i = 0; i < this.selectedCounters.length; i++) {
            _loop_1(i);
        }
        var self = this;
        this.timerValue = setTimeout(function () {
            console.log('Refreshing');
            self.refreshGraphs();
        }, 10000);
    };
    StatusComponent.prototype.toggleVisible = function (item) {
        console.log(item);
        item.itemsVisible = !item.itemsVisible;
    };
    StatusComponent.prototype.toggleSelect = function (item) {
        item.selected = !item.selected;
        if (item.selected) {
            this.selectedCounters.push(item.countersId);
        }
        else {
            var nSelected = new Array();
            for (var i = 0; i < this.selectedCounters.length; i++) {
                if (this.selectedCounters[i] !== item.countersId) {
                    nSelected.push(this.selectedCounters[i]);
                }
            }
            this.selectedCounters = nSelected;
        }
        this.shownCounters = [];
        this.refreshGraphs();
    };
    StatusComponent.prototype.toggleSidebar = function () {
        this.showSideBar = !this.showSideBar;
    };
    StatusComponent.prototype.hideSidebar = function () {
        this.showSideBar = false;
    };
    StatusComponent.prototype.showSidebar = function () {
        this.showSideBar = true;
    };
    StatusComponent.prototype.chartClicked = function (event) {
    };
    // Private Methods
    StatusComponent.prototype.createCountersTree = function (data) {
        var counters = new Array();
        var _loop_2 = function (i) {
            var currentItem = data[i];
            var appItem = counters.find(function (item) { return item.applicationName === currentItem.application; });
            if (appItem === undefined) {
                appItem = {
                    applicationName: currentItem.application,
                    items: new Array(),
                    itemsVisible: false
                };
                counters.push(appItem);
                counters.sort(function (a, b) { return a.applicationName < b.applicationName ? -1 : 1; });
            }
            var kindItem = appItem.items.find(function (item) { return item.kindName === currentItem.kind; });
            if (kindItem === undefined) {
                kindItem = {
                    kindName: currentItem.kind,
                    items: new Array(),
                    itemsVisible: false
                };
                appItem.items.push(kindItem);
                appItem.items.sort(function (a, b) { return a.kindName < b.kindName ? -1 : 1; });
            }
            var categoryItem = kindItem.items.find(function (item) { return item.categoryName === currentItem.category; });
            if (categoryItem === undefined) {
                categoryItem = {
                    categoryName: currentItem.category,
                    items: new Array(),
                    itemsVisible: false
                };
                kindItem.items.push(categoryItem);
                kindItem.items.sort(function (a, b) { return a.categoryName < b.categoryName ? -1 : 1; });
            }
            categoryItem.items.push(Object.assign(currentItem, { selected: false, lastData: [], barChartLabels: [], barChartData: [] }));
        };
        for (var i = 0; i < data.length; i++) {
            _loop_2(i);
        }
        return counters;
    };
    StatusComponent.prototype.updateParams = function () {
        this._queryParams.env = _environments_environment__WEBPACK_IMPORTED_MODULE_3__["environment"].name;
        this._router.navigate([], {
            relativeTo: this._activatedRoute,
            queryParams: this._queryParams,
            replaceUrl: true
        });
    };
    StatusComponent = __decorate([
        Object(_angular_core__WEBPACK_IMPORTED_MODULE_1__["Component"])({
            template: __webpack_require__(/*! ./status.component.html */ "./src/app/views/diagnostics/status.component.html")
        }),
        __metadata("design:paramtypes", [_services_api_api_query_service__WEBPACK_IMPORTED_MODULE_2__["QueryService"],
            _angular_router__WEBPACK_IMPORTED_MODULE_0__["ActivatedRoute"],
            _angular_router__WEBPACK_IMPORTED_MODULE_0__["Router"]])
    ], StatusComponent);
    return StatusComponent;
}());

var AppCounters = /** @class */ (function () {
    function AppCounters() {
        this.itemsVisible = false;
    }
    return AppCounters;
}());
var KindCounters = /** @class */ (function () {
    function KindCounters() {
        this.itemsVisible = false;
    }
    return KindCounters;
}());
var CategoryCounters = /** @class */ (function () {
    function CategoryCounters() {
        this.itemsVisible = false;
    }
    return CategoryCounters;
}());


/***/ }),

/***/ "./src/app/views/diagnostics/tracedetails.component.html":
/*!***************************************************************!*\
  !*** ./src/app/views/diagnostics/tracedetails.component.html ***!
  \***************************************************************/
/*! no static exports found */
/***/ (function(module, exports) {

module.exports = "<ol class=\"breadcrumb breadcrumb-body\">\n  <table>\n    <tr>\n      <td class=\"breadcrumb-icon\">\n        <span class=\"back-link fa-pointer\" (click)=\"goBack()\">\n          <i class=\"fa fa-chevron-left \">&nbsp;&nbsp;</i>Back\n        </span>\n      </td>\n      <td class=\"breadcrumb-title\"><strong>Trace Group:</strong>&nbsp;&nbsp;{{group}}</td>\n    </tr>\n  </table>\n</ol>\n\n<div>\n\n  <!-- No Group Alert -->\n  <div class=\"card\" *ngIf=\"group == null\">\n    <div class=\"card-body\">\n      <alert type=\"info\" class=\"no-bottom-margin-alert\">\n        <strong>Ups!, something went wrong</strong>\n        <br/> You must select a trace group in order to view it's details.\n      </alert>\n    </div>\n  </div>\n\n  <div class=\"row\">\n    <div class=\"col-lg-12\">\n      <div class=\"card\">\n        <!-- <div class=\"card-header\">\n          <i class=\"fa fa-cubes\"></i>\n          <strong>Group:</strong> {{group}}\n        </div> -->\n        <div class=\"card-body\">\n          <i class=\"fa fa-pointer fa-refresh float-right\" (click)=\"updateData()\"></i>\n          <table class=\"table table-sm table-striped table-trace\">\n            <thead>\n              <tr>\n                <th style=\"width: 5px\"></th>\n                <th style=\"width: max-content;\">Name</th>\n                <th style=\"width: 380px\">Application</th>\n                <th style=\"width: 90px\">Machine</th>\n                <th style=\"width: 180px\">Tags</th>\n                <th style=\"width: 90px\">Timestamp</th>\n                <th style=\"width: 80px\"></th>\n              </tr>\n            </thead>\n            <tbody>\n              <tr *ngFor=\"let item of items\">\n                <td>\n                    <i class=\"fa fa-big\" [ngClass]=\"item.application.indexOf('Api') > -1 ? 'fa-cloud' :\n                        item.application.indexOf('Engine') > -1 ? 'fa-cog' :\n                        item.application.indexOf('Provider') > -1 ? 'fa-plane' :\n                        'fa-cube'\"></i>\n                </td>\n                <td class=\"breakWord\">{{item.name}}</td>\n                <td class=\"breakWord\">{{item.application}}</td>\n                <td>{{item.machine}}</td>\n                <td>\n                  <button *ngFor=\"let tag of item.tagsArray\" class=\"btn badge badge-tag\" [ngClass]=\"{\n                      'button-info' : tag.key != 'Status',\n                      'button-success' : tag.key == 'Status' && tag.value == 'Success',\n                      'button-warning' : tag.key == 'Status' && tag.value == 'Warning',\n                      'button-error' : tag.key == 'Status' && tag.value == 'Error'\n                    }\" [popover]=\"tag.value\" [popoverTitle]=\"tag.key\" placement=\"top\" [outsideClick]=\"true\">\n                    {{tag.key}}\n                  </button>\n                </td>\n                <td>{{item.timestamp | date:'HH:mm:ss.SSS'}}</td>\n                <td>\n                  <button class=\"btn badge badge-tag button-ser\" (click)=\"showJsonData(item.id, item.name)\" [ngClass]=\"item.cssClass\">Json</button>\n                  <button class=\"btn badge badge-tag button-ser\" (click)=\"showXmlData(item.id, item.name)\" [ngClass]=\"item.cssClass\">Xml</button>\n                </td>\n              </tr>\n            </tbody>\n          </table>\n        </div>\n      </div>\n    </div>\n  </div>\n\n</div>\n\n\n\n<!-- Trace View -->\n<div bsModal #traceModal=\"bs-modal\" class=\"modal fade\" tabindex=\"-1\" role=\"dialog\" aria-labelledby=\"myModalLabel\" aria-hidden=\"true\">\n  <div class=\"modal-dialog modal-primary modal-code\" role=\"document\">\n    <div class=\"modal-content\">\n      <div class=\"modal-header\">\n        <h5 class=\"modal-title\">\n          <strong>Trace Object View: </strong> {{traceName}}</h5>\n        <button type=\"button\" class=\"close\" (click)=\"traceModal.hide()\" aria-label=\"Close\">\n          <span aria-hidden=\"true\">&times;</span>\n        </button>\n      </div>\n      <div class=\"modal-body\">\n        <ngx-codemirror></ngx-codemirror>\n      </div>\n    </div>\n  </div>\n</div>\n"

/***/ }),

/***/ "./src/app/views/diagnostics/tracedetails.component.ts":
/*!*************************************************************!*\
  !*** ./src/app/views/diagnostics/tracedetails.component.ts ***!
  \*************************************************************/
/*! exports provided: TraceDetailsComponent */
/***/ (function(module, __webpack_exports__, __webpack_require__) {

"use strict";
__webpack_require__.r(__webpack_exports__);
/* harmony export (binding) */ __webpack_require__.d(__webpack_exports__, "TraceDetailsComponent", function() { return TraceDetailsComponent; });
/* harmony import */ var _angular_router__WEBPACK_IMPORTED_MODULE_0__ = __webpack_require__(/*! @angular/router */ "./node_modules/@angular/router/fesm5/router.js");
/* harmony import */ var _angular_core__WEBPACK_IMPORTED_MODULE_1__ = __webpack_require__(/*! @angular/core */ "./node_modules/@angular/core/fesm5/core.js");
/* harmony import */ var _angular_common__WEBPACK_IMPORTED_MODULE_2__ = __webpack_require__(/*! @angular/common */ "./node_modules/@angular/common/fesm5/common.js");
/* harmony import */ var _services_api_api_query_service__WEBPACK_IMPORTED_MODULE_3__ = __webpack_require__(/*! ../../services/api/api/query.service */ "./src/app/services/api/api/query.service.ts");
/* harmony import */ var _environments_environment__WEBPACK_IMPORTED_MODULE_4__ = __webpack_require__(/*! ../../../environments/environment */ "./src/environments/environment.ts");
/* harmony import */ var ngx_bootstrap_modal__WEBPACK_IMPORTED_MODULE_5__ = __webpack_require__(/*! ngx-bootstrap/modal */ "./node_modules/ngx-bootstrap/modal/index.js");
/* harmony import */ var _nomadreservations_ngx_codemirror__WEBPACK_IMPORTED_MODULE_6__ = __webpack_require__(/*! @nomadreservations/ngx-codemirror */ "./node_modules/@nomadreservations/ngx-codemirror/fesm5/nomadreservations-ngx-codemirror.js");
var __decorate = (undefined && undefined.__decorate) || function (decorators, target, key, desc) {
    var c = arguments.length, r = c < 3 ? target : desc === null ? desc = Object.getOwnPropertyDescriptor(target, key) : desc, d;
    if (typeof Reflect === "object" && typeof Reflect.decorate === "function") r = Reflect.decorate(decorators, target, key, desc);
    else for (var i = decorators.length - 1; i >= 0; i--) if (d = decorators[i]) r = (c < 3 ? d(r) : c > 3 ? d(target, key, r) : d(target, key)) || r;
    return c > 3 && r && Object.defineProperty(target, key, r), r;
};
var __metadata = (undefined && undefined.__metadata) || function (k, v) {
    if (typeof Reflect === "object" && typeof Reflect.metadata === "function") return Reflect.metadata(k, v);
};







var TraceDetailsComponent = /** @class */ (function () {
    function TraceDetailsComponent(_queryService, _activatedRoute, _router, _codeMirror, _location) {
        this._queryService = _queryService;
        this._activatedRoute = _activatedRoute;
        this._router = _router;
        this._codeMirror = _codeMirror;
        this._location = _location;
        this.group = null;
        this.items = [];
        this.applications = [];
    }
    TraceDetailsComponent.prototype.ngOnInit = function () {
        this._params = Object.assign({}, this._activatedRoute.snapshot.params);
        this._queryParams = Object.assign({}, this._activatedRoute.snapshot.queryParams);
        if (this._params.group !== undefined) {
            this.group = this._params.group;
            this.loadData();
        }
        else {
            this._location.back();
        }
    };
    TraceDetailsComponent.prototype.updateData = function () {
        this.updateParams();
        this.loadData();
    };
    TraceDetailsComponent.prototype.loadData = function () {
        var _this = this;
        this._queryService.apiQueryByEnvironmentTracesByGroupNameGet(_environments_environment__WEBPACK_IMPORTED_MODULE_4__["environment"].name, this.group).subscribe(function (lstTraces) {
            // Parse Tags and create TagsArray and Group Applications
            _this.applications.length = 0;
            _this.items.length = 0;
            for (var i = 0; i < lstTraces.length; i++) {
                var item = lstTraces[i];
                var itemTags = item.tags.split(', ');
                var tags = [];
                for (var it = 0; it < itemTags.length; it++) {
                    var itemTagItem = itemTags[it].split(': ');
                    tags.push({ key: itemTagItem[0], value: itemTagItem[1] });
                }
                if (_this.applications.indexOf(item.application) === -1) {
                    _this.applications.push(item.application);
                }
                _this.items.push(Object.assign(item, {
                    tagsArray: tags,
                    cssClass: 'trace-application-bgcolor' + _this.applications.indexOf(item.application)
                }));
            }
        });
    };
    TraceDetailsComponent.prototype.showXmlData = function (id, name) {
        var _this = this;
        this.traceName = name;
        this.traceModal.show();
        this._queryService.apiQueryByEnvironmentTracesXmlByIdGet(_environments_environment__WEBPACK_IMPORTED_MODULE_4__["environment"].name, id).subscribe(function (x) {
            _this.traceObject = x;
            _this._codeMirror.instance$.subscribe(function (editor) {
                editor.setOption('mode', 'application/xml');
                if (x.startsWith('{')) {
                    editor.setOption('mode', 'application/json');
                }
                editor.setOption('theme', 'material');
                editor.setOption('readOnly', true);
                editor.setOption('lineNumbers', true);
                editor.setOption('matchBrackets', true);
                editor.setOption('foldGutter', true);
                editor.setOption('gutters', ['CodeMirror-linenumbers', 'CodeMirror-foldgutter']);
                editor.setOption('extraKeys', { 'Alt-F': 'findPersistent' });
                editor.setValue(_this.traceObject);
                editor.setSize('100%', '700px');
                editor.getDoc().setCursor({ line: 0, ch: 0 });
                editor.getDoc().setSelection({ line: 0, ch: 0 }, { line: 0, ch: 0 }, { scroll: true });
                editor.scrollTo(0, 0);
                setTimeout(function () { return editor.refresh(); }, 200);
            });
        });
    };
    TraceDetailsComponent.prototype.showJsonData = function (id, name) {
        var _this = this;
        this.traceName = name;
        this.traceModal.show();
        this._queryService.apiQueryByEnvironmentTracesJsonByIdGet(_environments_environment__WEBPACK_IMPORTED_MODULE_4__["environment"].name, id).subscribe(function (x) {
            _this.traceObject = x;
            _this._codeMirror.instance$.subscribe(function (editor) {
                editor.setOption('mode', 'application/json');
                if (x.startsWith('<?xml')) {
                    editor.setOption('mode', 'application/xml');
                }
                editor.setOption('theme', 'material');
                editor.setOption('readOnly', true);
                editor.setOption('lineNumbers', true);
                editor.setOption('matchBrackets', true);
                editor.setOption('foldGutter', true);
                editor.setOption('gutters', ['CodeMirror-linenumbers', 'CodeMirror-foldgutter']);
                editor.setOption('extraKeys', { 'Alt-F': 'findPersistent' });
                editor.setValue(_this.traceObject);
                editor.setSize('100%', '700px');
                editor.getDoc().setCursor({ line: 0, ch: 0 });
                editor.getDoc().setSelection({ line: 0, ch: 0 }, { line: 0, ch: 0 }, { scroll: true });
                editor.scrollTo(0, 0);
                setTimeout(function () { return editor.refresh(); }, 200);
            });
        });
    };
    TraceDetailsComponent.prototype.goBack = function () {
        this._location.back();
    };
    // Private Methods
    TraceDetailsComponent.prototype.updateParams = function () {
        this._queryParams.env = _environments_environment__WEBPACK_IMPORTED_MODULE_4__["environment"].name;
        this._params.group = this.group;
        this._router.navigate([], {
            relativeTo: this._activatedRoute,
            queryParams: this._queryParams,
            replaceUrl: true
        });
    };
    __decorate([
        Object(_angular_core__WEBPACK_IMPORTED_MODULE_1__["ViewChild"])('traceModal'),
        __metadata("design:type", ngx_bootstrap_modal__WEBPACK_IMPORTED_MODULE_5__["ModalDirective"])
    ], TraceDetailsComponent.prototype, "traceModal", void 0);
    TraceDetailsComponent = __decorate([
        Object(_angular_core__WEBPACK_IMPORTED_MODULE_1__["Component"])({
            template: __webpack_require__(/*! ./tracedetails.component.html */ "./src/app/views/diagnostics/tracedetails.component.html")
        }),
        __metadata("design:paramtypes", [_services_api_api_query_service__WEBPACK_IMPORTED_MODULE_3__["QueryService"], _angular_router__WEBPACK_IMPORTED_MODULE_0__["ActivatedRoute"], _angular_router__WEBPACK_IMPORTED_MODULE_0__["Router"], _nomadreservations_ngx_codemirror__WEBPACK_IMPORTED_MODULE_6__["CodemirrorService"], _angular_common__WEBPACK_IMPORTED_MODULE_2__["Location"]])
    ], TraceDetailsComponent);
    return TraceDetailsComponent;
}());



/***/ }),

/***/ "./src/app/views/diagnostics/traces.component.html":
/*!*********************************************************!*\
  !*** ./src/app/views/diagnostics/traces.component.html ***!
  \*********************************************************/
/*! no static exports found */
/***/ (function(module, exports) {

module.exports = "<ol class=\"breadcrumb breadcrumb-body\">\r\n  <table>\r\n    <tr>\r\n      <td class=\"breadcrumb-icon\"></td>\r\n      <td class=\"breadcrumb-title\">\r\n        <strong>Day Traces</strong> -\r\n        <input type=\"text\" placeholder=\"Showing dates\" class=\"widget-date widget-day fa-pointer\" bsDatepicker [bsConfig]=\"bsConfig\" [bsValue]=\"bsValue\"\r\n              [(ngModel)]=\"bsValue\" (ngModelChange)=\"changeDate()\" />\r\n            <span class=\"fa-pointer float-right\" (click)=\"updateData()\">\r\n                <i class=\"fa fa-refresh\"></i>&nbsp;\r\n                <span>Refresh</span>\r\n            </span>\r\n      </td>\r\n    </tr>\r\n  </table>\r\n</ol>\r\n\r\n<div>\r\n\r\n  <!-- No Data Alert -->\r\n  <div class=\"card\" *ngIf=\"traceData != null && traceData?.data.length == 0\">\r\n    <div class=\"card-body\">\r\n      <alert type=\"info\" class=\"no-bottom-margin-alert\">\r\n        <strong>Ups!, there is not data for the selected date</strong>\r\n        <br/> Please select another date to get results.\r\n      </alert>\r\n    </div>\r\n  </div>\r\n\r\n  <!-- Tablas de Datos -->\r\n  <div class=\"row\" *ngIf=\"traceData != null && traceData.data.length > 0\">\r\n    <div class=\"col-lg-12\">\r\n      <div class=\"card\">\r\n        <div class=\"card-body\">\r\n\r\n          <span class=\" float-right\">\r\n              <span *ngIf=\"traceData != null\">{{traceData.totalResults}} items &nbsp;</span>\r\n          </span>\r\n\r\n          <table class=\"table table-sm table-striped table-log\">\r\n            <thead>\r\n              <tr>\r\n                <th style=\"width: 165px;\">Start</th>\r\n                <th style=\"width: 165px;\">End</th>\r\n                <th style=\"width: 100px;\">Time</th>\r\n                <th style=\"width: max-content;\">Group name</th>\r\n                <th style=\"width: 90px;\">Quantity</th>\r\n                <th style=\"width: 90px;\"></th>\r\n              </tr>\r\n            </thead>\r\n            <tbody>\r\n              <tr *ngFor=\"let dataItem of traceData?.data\">\r\n                <td>{{dataItem.start | date:'dd/MM/yy HH:mm:ss.SSS'}}</td>\r\n                <td>{{dataItem.end | date:'dd/MM/yy HH:mm:ss.SSS'}}</td>\r\n                <td>{{ getTimeDiff(dataItem.end, dataItem.start) }}</td>\r\n                <td class=\"breakWord\">{{dataItem.group}}</td>\r\n                <td>{{dataItem.count}}</td>\r\n                <td>\r\n                  <span class=\"badge\"\r\n                    [ngClass]=\"{ 'button-success': !dataItem.hasErrors, 'button-error': dataItem.hasErrors }\"\r\n                    [routerLink]=\"['/diagnostics/search']\" [queryParams]=\"{\r\n                      term: dataItem.group,\r\n                      env: environmentName,\r\n                      fromDate: dataItem.start,\r\n                      toDate: dataItem.end\r\n                    }\">Show Log and Traces</span>\r\n                </td>\r\n              </tr>\r\n            </tbody>\r\n          </table>\r\n        </div>\r\n        <div class=\"card-footer\" *ngIf=\"traceData?.totalPages > 1\">\r\n          <ul class=\"pagination\">\r\n            <li class=\"page-item\">\r\n              <a class=\"page-link\" (click)=\"goToPage(traceData?.pageNumber - 1)\">Prev</a>\r\n            </li>\r\n            <li *ngFor=\"let i of totalPagesArray\" class=\"page-item\" [ngClass]=\"{ 'active' : i == traceData?.pageNumber }\">\r\n              <a class=\"page-link\" (click)=\"goToPage(i)\">{{i+1}}</a>\r\n            </li>\r\n            <li class=\"page-item\">\r\n              <a class=\"page-link\" (click)=\"goToPage(traceData?.pageNumber + 1)\">Next</a>\r\n            </li>\r\n          </ul>\r\n        </div>\r\n      </div>\r\n    </div>\r\n  </div>\r\n\r\n\r\n</div>\r\n"

/***/ }),

/***/ "./src/app/views/diagnostics/traces.component.ts":
/*!*******************************************************!*\
  !*** ./src/app/views/diagnostics/traces.component.ts ***!
  \*******************************************************/
/*! exports provided: TracesComponent */
/***/ (function(module, __webpack_exports__, __webpack_require__) {

"use strict";
__webpack_require__.r(__webpack_exports__);
/* harmony export (binding) */ __webpack_require__.d(__webpack_exports__, "TracesComponent", function() { return TracesComponent; });
/* harmony import */ var _angular_router__WEBPACK_IMPORTED_MODULE_0__ = __webpack_require__(/*! @angular/router */ "./node_modules/@angular/router/fesm5/router.js");
/* harmony import */ var _angular_core__WEBPACK_IMPORTED_MODULE_1__ = __webpack_require__(/*! @angular/core */ "./node_modules/@angular/core/fesm5/core.js");
/* harmony import */ var _services_api_api_query_service__WEBPACK_IMPORTED_MODULE_2__ = __webpack_require__(/*! ../../services/api/api/query.service */ "./src/app/services/api/api/query.service.ts");
/* harmony import */ var _environments_environment__WEBPACK_IMPORTED_MODULE_3__ = __webpack_require__(/*! ../../../environments/environment */ "./src/environments/environment.ts");
/* harmony import */ var ngx_bootstrap_datepicker__WEBPACK_IMPORTED_MODULE_4__ = __webpack_require__(/*! ngx-bootstrap/datepicker */ "./node_modules/ngx-bootstrap/datepicker/index.js");
/* harmony import */ var ngx_bootstrap_chronos__WEBPACK_IMPORTED_MODULE_5__ = __webpack_require__(/*! ngx-bootstrap/chronos */ "./node_modules/ngx-bootstrap/chronos/index.js");
/* harmony import */ var ngx_bootstrap_locale__WEBPACK_IMPORTED_MODULE_6__ = __webpack_require__(/*! ngx-bootstrap/locale */ "./node_modules/ngx-bootstrap/locale.js");
/* harmony import */ var ngx_bootstrap_chronos_test_chain__WEBPACK_IMPORTED_MODULE_7__ = __webpack_require__(/*! ngx-bootstrap/chronos/test/chain */ "./node_modules/ngx-bootstrap/chronos/test/chain.js");
var __decorate = (undefined && undefined.__decorate) || function (decorators, target, key, desc) {
    var c = arguments.length, r = c < 3 ? target : desc === null ? desc = Object.getOwnPropertyDescriptor(target, key) : desc, d;
    if (typeof Reflect === "object" && typeof Reflect.decorate === "function") r = Reflect.decorate(decorators, target, key, desc);
    else for (var i = decorators.length - 1; i >= 0; i--) if (d = decorators[i]) r = (c < 3 ? d(r) : c > 3 ? d(target, key, r) : d(target, key)) || r;
    return c > 3 && r && Object.defineProperty(target, key, r), r;
};
var __metadata = (undefined && undefined.__metadata) || function (k, v) {
    if (typeof Reflect === "object" && typeof Reflect.metadata === "function") return Reflect.metadata(k, v);
};







Object(ngx_bootstrap_chronos__WEBPACK_IMPORTED_MODULE_5__["defineLocale"])('en-gb', ngx_bootstrap_locale__WEBPACK_IMPORTED_MODULE_6__["enGbLocale"]);

var TracesComponent = /** @class */ (function () {
    function TracesComponent(_queryService, _localeService, _activatedRoute, _router) {
        this._queryService = _queryService;
        this._localeService = _localeService;
        this._activatedRoute = _activatedRoute;
        this._router = _router;
        this._currentPage = 0;
        this._pageSize = 50;
    }
    // Public Methods
    TracesComponent.prototype.ngOnInit = function () {
        var initialDate = new Date();
        this._queryParams = Object.assign({}, this._activatedRoute.snapshot.queryParams);
        if (this._queryParams.date !== undefined) {
            initialDate = ngx_bootstrap_chronos_test_chain__WEBPACK_IMPORTED_MODULE_7__["moment"].utc(this._queryParams.date, 'YYYY-MM-DD').toDate();
        }
        if (this._queryParams.page !== undefined) {
            this._currentPage = parseInt(this._queryParams.page, 0);
        }
        this.bsConfig = Object.assign({}, {
            containerClass: 'theme-dark-blue',
            maxDate: new Date(),
            showWeekNumbers: false
        });
        this.bsValue = initialDate;
        this._localeService.use('en-gb');
        this.updateParams();
        this.updateData();
    };
    TracesComponent.prototype.updateData = function () {
        var _this = this;
        if (_environments_environment__WEBPACK_IMPORTED_MODULE_3__["environment"].name === undefined || _environments_environment__WEBPACK_IMPORTED_MODULE_3__["environment"].name === null || _environments_environment__WEBPACK_IMPORTED_MODULE_3__["environment"].name.length === 0) {
            return;
        }
        this.environmentName = _environments_environment__WEBPACK_IMPORTED_MODULE_3__["environment"].name;
        this._queryService.apiQueryByEnvironmentTracesGet(_environments_environment__WEBPACK_IMPORTED_MODULE_3__["environment"].name, this.bsValue, this.bsValue, this._currentPage, this._pageSize).subscribe(function (item) {
            if (item === null) {
                return;
            }
            var maxPages = 10;
            _this.traceData = item;
            if (item.totalPages < maxPages) {
                _this.totalPagesArray = Array(item.totalPages).fill(0).map(function (a, i) { return i; });
            }
            else {
                var midPoint = maxPages / 2;
                if (item.pageNumber <= midPoint) {
                    _this.totalPagesArray = Array(maxPages).fill(0).map(function (a, i) { return i; });
                }
                else if (item.totalPages - item.pageNumber < midPoint) {
                    var startPoint_1 = item.totalPages - maxPages;
                    _this.totalPagesArray = Array(maxPages).fill(0).map(function (a, i) { return startPoint_1 + i; });
                }
                else {
                    var startPoint_2 = item.pageNumber - midPoint;
                    _this.totalPagesArray = Array(maxPages).fill(0).map(function (a, i) { return startPoint_2 + i; });
                }
            }
        });
    };
    TracesComponent.prototype.changeDate = function () {
        this._currentPage = 0;
        this.updateParams();
        this.updateData();
    };
    TracesComponent.prototype.goToPage = function (page) {
        if (page === void 0) { page = 0; }
        if (page < 0) {
            return;
        }
        if (page > this.totalPagesArray[this.totalPagesArray.length - 1]) {
            return;
        }
        this._currentPage = page;
        this.updateParams();
        this.updateData();
    };
    TracesComponent.prototype.getTimeDiff = function (end, start) {
        var timeInSeconds = (Object(ngx_bootstrap_chronos_test_chain__WEBPACK_IMPORTED_MODULE_7__["moment"])(end).valueOf() - Object(ngx_bootstrap_chronos_test_chain__WEBPACK_IMPORTED_MODULE_7__["moment"])(start).valueOf()) / 1000;
        var minutes = Math.floor(timeInSeconds / 60);
        var seconds = Math.round(timeInSeconds - (minutes * 60));
        return (minutes > 9 ? minutes : "0" + minutes) + ":" + (seconds > 9 ? seconds : "0" + seconds);
    };
    // Private Methods
    TracesComponent.prototype.updateParams = function () {
        this._queryParams.env = _environments_environment__WEBPACK_IMPORTED_MODULE_3__["environment"].name;
        this._queryParams.date = Object(ngx_bootstrap_chronos_test_chain__WEBPACK_IMPORTED_MODULE_7__["moment"])(this.bsValue).format('YYYY-MM-DD');
        this._queryParams.page = this._currentPage;
        this._router.navigate([], {
            relativeTo: this._activatedRoute,
            queryParams: this._queryParams,
            replaceUrl: true
        });
    };
    TracesComponent = __decorate([
        Object(_angular_core__WEBPACK_IMPORTED_MODULE_1__["Component"])({
            template: __webpack_require__(/*! ./traces.component.html */ "./src/app/views/diagnostics/traces.component.html")
        }),
        __metadata("design:paramtypes", [_services_api_api_query_service__WEBPACK_IMPORTED_MODULE_2__["QueryService"], ngx_bootstrap_datepicker__WEBPACK_IMPORTED_MODULE_4__["BsLocaleService"], _angular_router__WEBPACK_IMPORTED_MODULE_0__["ActivatedRoute"], _angular_router__WEBPACK_IMPORTED_MODULE_0__["Router"]])
    ], TracesComponent);
    return TracesComponent;
}());



/***/ })

}]);
//# sourceMappingURL=views-diagnostics-diagnostics-module.js.map