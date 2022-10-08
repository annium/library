// noinspection JSUnusedGlobalSymbols

import js from '../interop/js.js'
import objectTracker from '../trackers/objectTracker.js'

/* properties */

// width
export const getWidth = (data: number): number => {
  return getEl(data).width
}
export const setWidth = (data: number): void => {
  getEl(data).width = js.readInt(data, 4)
}

// height
export const getHeight = (data: number): number => {
  return getEl(data).height
}
export const setHeight = (data: number): void => {
  getEl(data).height = js.readInt(data, 4)
}

// fillStyle
export const getFillStyle = (data: number): number => {
  return js.writeString(getContext(data).fillStyle.toString())
}
export const setFillStyle = (data: number): void => {
  getContext(data).fillStyle = js.readString(data, 4)
}

// strokeStyle
export const getStrokeStyle = (data: number): number => {
  return js.writeString(getContext(data).strokeStyle.toString())
}
export const setStrokeStyle = (data: number): void => {
  getContext(data).strokeStyle = js.readString(data, 4)
}

// lineCap
export const getLineCap = (data: number): number => {
  return js.writeString(getContext(data).lineCap)
}
export const setLineCap = (data: number): void => {
  getContext(data).lineCap = js.readString(data, 4) as CanvasLineCap
}

// lineJoin
export const getLineJoin = (data: number): number => {
  return js.writeString(getContext(data).lineJoin)
}
export const setLineJoin = (data: number): void => {
  getContext(data).lineJoin = js.readString(data, 4) as CanvasLineJoin
}

// lineWidth
export const getLineWidth = (data: number): number => {
  return getContext(data).lineWidth
}
export const setLineWidth = (data: number): void => {
  getContext(data).lineWidth = js.readInt(data, 4)
}

// lineDashOffset
export const getLineDashOffset = (data: number): number => {
  return getContext(data).lineDashOffset
}
export const setLineDashOffset = (data: number): void => {
  getContext(data).lineDashOffset = js.readInt(data, 4)
}

// miterLimit
export const getMiterLimit = (data: number): number => {
  return getContext(data).miterLimit
}
export const setMiterLimit = (data: number): void => {
  getContext(data).miterLimit = js.readInt(data, 4)
}

// lineDash
export const getLineDash = (data: number): number => {
  return js.writeString(getContext(data).getLineDash().map(Math.round).join(','))
}
export const setLineDash = (data: number): void => {
  getContext(data).setLineDash(js.readString(data, 4).split(',').map(Number))
}

// font
export const getFont = (data: number): number => {
  return js.writeString(getContext(data).font.toString())
}
export const setFont = (data: number): void => {
  getContext(data).font = js.readString(data, 4)
}

// textAlign
export const getTextAlign = (data: number): number => {
  return js.writeString(getContext(data).textAlign)
}
export const setTextAlign = (data: number): void => {
  getContext(data).textAlign = js.readString(data, 4) as CanvasTextAlign
}

// textBaseline
export const getTextBaseline = (data: number): number => {
  return js.writeString(getContext(data).textBaseline)
}
export const setTextBaseline = (data: number): void => {
  getContext(data).textBaseline = js.readString(data, 4) as CanvasTextBaseline
}

/* methods */

// rects
export const clearRect = (data: number): void => {
  getContext(data).clearRect(js.readInt(data, 4), js.readInt(data, 8), js.readInt(data, 12), js.readInt(data, 16))
}
export const fillRect = (data: number): void => {
  getContext(data).fillRect(js.readInt(data, 4), js.readInt(data, 8), js.readInt(data, 12), js.readInt(data, 16))
}

// paths
export const beginPath = (data: number): void => {
  getContext(data).beginPath()
}
export const closePath = (data: number): void => {
  getContext(data).closePath()
}
export const moveTo = (data: number): void => {
  getContext(data).moveTo(js.readFloat(data, 4), js.readFloat(data, 8))
}
export const lineTo = (data: number): void => {
  getContext(data).lineTo(js.readFloat(data, 4), js.readFloat(data, 8))
}
export const arc = (data: number): void => {
  getContext(data).arc(js.readFloat(data, 4), js.readFloat(data, 8), js.readFloat(data, 12), js.readFloat(data, 16), js.readFloat(data, 20), !!js.readShort(data, 24))
}
export const stroke = (data: number): void => {
  getContext(data).stroke()
}

// areas
export const fill = (data: number): void => {
  getContext(data).fill()
}

// text
export const fillText = (data: number): void => {
  const ctx = getContext(data);
  const maxWidth = js.readInt(data, 16)
  if (maxWidth)
    ctx.fillText(js.readString(data, 4), js.readInt(data, 8), js.readInt(data, 12), maxWidth)
  else
    ctx.fillText(js.readString(data, 4), js.readInt(data, 8), js.readInt(data, 12))
}
export const strokeText = (data: number): void => {
  const ctx = getContext(data);
  const maxWidth = js.readInt(data, 16)
  if (maxWidth)
    ctx.strokeText(js.readString(data, 4), js.readInt(data, 8), js.readInt(data, 12), maxWidth)
  else
    ctx.strokeText(js.readString(data, 4), js.readInt(data, 8), js.readInt(data, 12))
}
export const measureTextWidth = (data: number): number => {
  return Math.ceil(getContext(data).measureText(js.readString(data, 4)).width)
}
export const measureTextHeight = (data: number): number => {
  const metrics = getContext(data).measureText(js.readString(data, 4));
  return Math.ceil(metrics.actualBoundingBoxAscent + metrics.actualBoundingBoxDescent);
}

// state
export const save = (data: number): void => {
  getContext(data).save();
}
export const restore = (data: number): void => {
  getContext(data).restore();
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

  newCtx.imageSmoothingEnabled = true;
  contexts.set(el, newCtx)

  return newCtx
}

function getEl(data: number): HTMLCanvasElement {
  return objectTracker.get<HTMLCanvasElement>(js.readString(data, 0))
}
