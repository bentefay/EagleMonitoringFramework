declare namespace ReactGridLayout {

    interface ItemProps {
        // Key of child if not index into layouts array
        i?: string;

        // These are all in grid units, not pixels
        x: number;
        y: number;
        w: number;
        h: number;
        minW?: number;
        maxW?: number;
        minH?: number;
        maxH?: number;

        // If true, equal to `isDraggable: false, isResizable: false`.
        static?: boolean;
        // If false, will not be draggable. Overrides `static`.
        isDraggable?: boolean;
        // If false, will not be resizable. Overrides `static`.
        isResizable?: boolean;
        className?: string;
        // Selector for draggable handle
        handle?: string;
        // Selector for draggable cancel (see react-draggable)
        cancel?: string;
    }

    type Callback = { (layout: ItemProps[], oldItem, newItem, placeholder, e, element): void };

    interface CommonProps {

        className?: string;

        // If true, the container height swells and contracts to fit contents. Default = true.
        autoSize?: boolean;

        // Default = {lg: 1200, md: 996, sm: 768, xs: 480, xxs: 0}
        breakpoints?: { [breakPointName: string]: number };

        // A selector that will not be draggable.
        draggableCancel?: string;
        // A selector for the draggable handler
        draggableHandle?: string;

        // If true, the layout will compact vertically. Default = true.
        verticalCompact?: boolean;

        // Layout is an array of object with the format:
        // {x: Number, y: Number, w: Number, h: Number}
        // The index into the layout must match the key used on each item component.
        // If you choose to use custom keys, you can specify that key in the layout
        // array objects like so:
        // {i: String, x: Number, y: Number, w: Number, h: Number}
        layout?: ItemProps[],

        // This allows setting the initial width on the server side.
        width?: number;

        // Margin between items [x, y] in px. Default = [10, 10]
        margin?: number[];

        // Rows have a static height, but you can change this based on breakpoints
        // if you like. Default = 150
        rowHeight?: number;

        // Flags

        // Default = true.
        isDraggable?: boolean;
        // Default = true.
        isResizable?: boolean;
        // Uses CSS3 translate() instead of position top/left.
        // This makes about 6x faster paint performance. Default = true.
        useCSSTransforms?: boolean;

        // If false, you should supply width yourself. Good if you want to debounce
        // resize events or reuse a handler from somewhere else. Default = true.
        listenToWindowResize?: boolean;

        // Callbacks

        // All callbacks below have signature (layout, oldItem, newItem, placeholder, e, element).
        // 'start' and 'stop' callbacks pass `undefined` for 'placeholder'.

        // Calls when drag starts.
        onDragStart?: Callback;
        // Calls on each drag movement.
        onDrag?: Callback;
        // Calls when drag is complete.
        onDragStop?: Callback;
        // Calls when resize starts.
        onResizeStart?: Callback;
        // Calls when resize movement happens.
        onResize?: Callback;
        // Calls when resize is complete.
        onResizeStop?: Callback;

    }

    interface ResponsiveProps extends CommonProps {

        // COPY AND PASTE

        className?: string;

        // If true, the container height swells and contracts to fit contents. Default = true.
        autoSize?: boolean;

        // Default = {lg: 1200, md: 996, sm: 768, xs: 480, xxs: 0}
        breakpoints?: { [breakPointName: string]: number };

        // A selector that will not be draggable.
        draggableCancel?: string;
        // A selector for the draggable handler
        draggableHandle?: string;

        // If true, the layout will compact vertically. Default = true.
        verticalCompact?: boolean;

        // Layout is an array of object with the format:
        // {x: Number, y: Number, w: Number, h: Number}
        // The index into the layout must match the key used on each item component.
        // If you choose to use custom keys, you can specify that key in the layout
        // array objects like so:
        // {i: String, x: Number, y: Number, w: Number, h: Number}
        layout?: ItemProps[],

        // This allows setting the initial width on the server side.
        width?: number;

        // Margin between items [x, y] in px. Default = [10, 10]
        margin?: number[];

        // Rows have a static height, but you can change this based on breakpoints
        // if you like. Default = 150
        rowHeight?: number;

        // Flags

        // Default = true.
        isDraggable?: boolean;
        // Default = true.
        isResizable?: boolean;
        // Uses CSS3 translate() instead of position top/left.
        // This makes about 6x faster paint performance. Default = true.
        useCSSTransforms?: boolean;

        // If false, you should supply width yourself. Good if you want to debounce
        // resize events or reuse a handler from somewhere else. Default = true.
        listenToWindowResize?: boolean;

        // Callbacks

        // All callbacks below have signature (layout, oldItem, newItem, placeholder, e, element).
        // 'start' and 'stop' callbacks pass `undefined` for 'placeholder'.

        // Calls when drag starts.
        onDragStart?: Callback;
        // Calls on each drag movement.
        onDrag?: Callback;
        // Calls when drag is complete.
        onDragStop?: Callback;
        // Calls when resize starts.
        onResizeStart?: Callback;
        // Calls when resize movement happens.
        onResize?: Callback;
        // Calls when resize is complete.
        onResizeStop?: Callback;

        // END COPY AND PASTE

        // # of cols. This is a breakpoint -> cols map, e.g. {lg: 12, md: 10, ...}
        cols?: { [breakPointName: string]: number };

        // layouts is an object mapping breakpoints to layouts.
        // e.g. {lg: Layout, md: Layout, ...}
        layouts: { [breakPointName: string]: ItemProps[] };

        // Callbacks

        // Calls back with breakpoint and new # cols
        onBreakpointChange: { (breakPointName: string, cols: number): void };

        // Callback so you can save the layout.
        // Calls back with (currentLayout, allLayouts). allLayouts are keyed by breakpoint.
        onLayoutChange: { (currentLayout: ItemProps[], allLayouts: { [breakPointName: string]: ItemProps[] }): void };

        // Callback when the width changes, so you can modify the layout as needed.
        // Calls back with (containerWidth, margin, cols)
        onWidthChange: { (containerWidth: number, margin: number[], cols: number) };

    }

    interface Props extends CommonProps {

        // COPY AND PASTE

        className?: string;

        // If true, the container height swells and contracts to fit contents. Default = true.
        autoSize?: boolean;

        // Default = {lg: 1200, md: 996, sm: 768, xs: 480, xxs: 0}
        breakpoints?: { [breakPointName: string]: number };

        // A selector that will not be draggable.
        draggableCancel?: string;
        // A selector for the draggable handler
        draggableHandle?: string;

        // If true, the layout will compact vertically. Default = true.
        verticalCompact?: boolean;

        // Layout is an array of object with the format:
        // {x: Number, y: Number, w: Number, h: Number}
        // The index into the layout must match the key used on each item component.
        // If you choose to use custom keys, you can specify that key in the layout
        // array objects like so:
        // {i: String, x: Number, y: Number, w: Number, h: Number}
        layout?: ItemProps[],

        // This allows setting the initial width on the server side.
        width?: number;

        // Margin between items [x, y] in px. Default = [10, 10]
        margin?: number[];

        // Rows have a static height, but you can change this based on breakpoints
        // if you like. Default = 150
        rowHeight?: number;

        // Flags

        // Default = true.
        isDraggable?: boolean;
        // Default = true.
        isResizable?: boolean;
        // Uses CSS3 translate() instead of position top/left.
        // This makes about 6x faster paint performance. Default = true.
        useCSSTransforms?: boolean;

        // If false, you should supply width yourself. Good if you want to debounce
        // resize events or reuse a handler from somewhere else. Default = true.
        listenToWindowResize?: boolean;

        // Callbacks

        // All callbacks below have signature (layout, oldItem, newItem, placeholder, e, element).
        // 'start' and 'stop' callbacks pass `undefined` for 'placeholder'.

        // Calls when drag starts.
        onDragStart?: Callback;
        // Calls on each drag movement.
        onDrag?: Callback;
        // Calls when drag is complete.
        onDragStop?: Callback;
        // Calls when resize starts.
        onResizeStart?: Callback;
        // Calls when resize movement happens.
        onResize?: Callback;
        // Calls when resize is complete.
        onResizeStop?: Callback;

        // END COPY AND PASTE

        // Number of columns in this layout. Default = 10
        cols?: number;

        // Callback so you can save the layout.
        // Calls back with (currentLayout) after every drag or resize stop.
        onLayoutChange?: { (currentLayout: ItemProps[]): void };
    }

    interface Layout {
    }

    interface Responsive extends __React.ClassicComponentClass<ResponsiveProps>, Layout {
    }
}

interface ReactGridLayout extends __React.ClassicComponentClass<ReactGridLayout.Props> {
}