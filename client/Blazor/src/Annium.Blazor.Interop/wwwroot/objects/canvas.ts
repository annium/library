// noinspection JSUnusedGlobalSymbols

import js from '../interop/js.js'
import objectTracker from '../trackers/objectTracker.js'

export default {
    /* properties */

    // width
    getWidth: function (data: number): number {
        return getEl(data).width
    },
    setWidth: function (data: number): void {
        getEl(data).width = js.readInt(data, 4)
    },

    // height
    getHeight: function (data: number): number {
        return getEl(data).height
    },
    setHeight: function (data: number): void {
        getEl(data).height = js.readInt(data, 4)
    },

    // fillStyle
    getFillStyle: function (data: number): number {
        return js.writeString(getContext(data).fillStyle.toString())
    },
    setFillStyle: function (data: number): void {
        getContext(data).fillStyle = js.readString(data, 4)
    },

    // strokeStyle
    getStrokeStyle: function (data: number): number {
        return js.writeString(getContext(data).strokeStyle.toString())
    },
    setStrokeStyle: function (data: number): void {
        getContext(data).strokeStyle = js.readString(data, 4)
    },

    // lineCap
    getLineCap: function (data: number): number {
        return js.writeString(getContext(data).lineCap)
    },
    setLineCap: function (data: number): void {
        getContext(data).lineCap = js.readString(data, 4) as CanvasLineCap
    },

    // lineJoin
    getLineJoin: function (data: number): number {
        return js.writeString(getContext(data).lineJoin)
    },
    setLineJoin: function (data: number): void {
        getContext(data).lineJoin = js.readString(data, 4) as CanvasLineJoin
    },

    // lineWidth
    getLineWidth: function (data: number): number {
        return getContext(data).lineWidth
    },
    setLineWidth: function (data: number): void {
        getContext(data).lineWidth = js.readInt(data, 4)
    },

    // lineDashOffset
    getLineDashOffset: function (data: number): number {
        return getContext(data).lineDashOffset
    },
    setLineDashOffset: function (data: number): void {
        getContext(data).lineDashOffset = js.readInt(data, 4)
    },

    // miterLimit
    getMiterLimit: function (data: number): number {
        return getContext(data).miterLimit
    },
    setMiterLimit: function (data: number): void {
        getContext(data).miterLimit = js.readInt(data, 4)
    },

    // lineDash
    getLineDash: function (data: number): number {
        return js.writeString(getContext(data).getLineDash().map(Math.round).join(','))
    },
    setLineDash: function (data: number): void {
        getContext(data).setLineDash(js.readString(data, 4).split(',').map(Number))
    },

    // font
    getFont: function (data: number): number {
        return js.writeString(getContext(data).font.toString())
    },
    setFont: function (data: number): void {
        getContext(data).font = js.readString(data, 4)
    },

    // textAlign
    getTextAlign: function (data: number): number {
        return js.writeString(getContext(data).textAlign)
    },
    setTextAlign: function (data: number): void {
        getContext(data).textAlign = js.readString(data, 4) as CanvasTextAlign
    },

    // textBaseline
    getTextBaseline: function (data: number): number {
        return js.writeString(getContext(data).textBaseline)
    },
    setTextBaseline: function (data: number): void {
        getContext(data).textBaseline = js.readString(data, 4) as CanvasTextBaseline
    },

    /* methods */

    // rects
    clearRect: function (data: number): void {
        getContext(data).clearRect(js.readInt(data, 4), js.readInt(data, 8), js.readInt(data, 12), js.readInt(data, 16))
    },
    fillRect: function (data: number): void {
        getContext(data).fillRect(js.readInt(data, 4), js.readInt(data, 8), js.readInt(data, 12), js.readInt(data, 16))
    },

    // paths
    beginPath: function (data: number): void {
        getContext(data).beginPath()
    },
    closePath: function (data: number): void {
        getContext(data).closePath()
    },
    moveTo: function (data: number): void {
        getContext(data).moveTo(js.readFloat(data, 4), js.readFloat(data, 8))
    },
    lineTo: function (data: number): void {
        getContext(data).lineTo(js.readFloat(data, 4), js.readFloat(data, 8))
    },
    stroke: function (data: number): void {
        getContext(data).stroke()
    },

    // text
    fillText: function (data: number): void {
        const ctx = getContext(data);
        const maxWidth = js.readInt(data, 16)
        if (maxWidth)
            ctx.fillText(js.readString(data, 4), js.readInt(data, 8), js.readInt(data, 12), maxWidth)
        else
            ctx.fillText(js.readString(data, 4), js.readInt(data, 8), js.readInt(data, 12))
    },
    strokeText: function (data: number): void {
        const ctx = getContext(data);
        const maxWidth = js.readInt(data, 16)
        if (maxWidth)
            ctx.strokeText(js.readString(data, 4), js.readInt(data, 8), js.readInt(data, 12), maxWidth)
        else
            ctx.strokeText(js.readString(data, 4), js.readInt(data, 8), js.readInt(data, 12))
    },
    measureTextWidth: function (data: number): number {
        return Math.ceil(getContext(data).measureText(js.readString(data, 4)).width)
    },
    measureTextHeight: function (data: number): number {
        const metrics = getContext(data).measureText(js.readString(data, 4));
        return Math.ceil(metrics.actualBoundingBoxAscent + metrics.actualBoundingBoxDescent);
    },

    // state
    save: function (data: number): void {
        getContext(data).save();
    },
    restore: function (data: number): void {
        getContext(data).restore();
    },
}

const contexts = new WeakMap<HTMLCanvasElement, CanvasRenderingContext2D>()

function getContext(data: number): CanvasRenderingContext2D {
    const el = getEl(data)
    const ctx = contexts.get(el);
    if (ctx)
        return ctx

    const newCtx = el.getContext('2d')
    if (!newCtx)
        throw new Error('failed to create 2d context')

    newCtx.imageSmoothingEnabled = false;
    contexts.set(el, newCtx)

    return newCtx
}

function getEl(data: number): HTMLCanvasElement {
    return objectTracker.get<HTMLCanvasElement>(js.readString(data, 0))
}
