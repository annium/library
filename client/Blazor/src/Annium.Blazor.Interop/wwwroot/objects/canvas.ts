// noinspection JSUnusedGlobalSymbols

import js from '../interop/js.js'
import objectTracker from '../trackers/objectTracker.js'

/* properties */

// width
export const getWidth = (data: number): number => {
  return getElObsolete(data).width
}
export const setWidth = (data: number): void => {
  getElObsolete(data).width = js.readInt(data, 4)
}

// height
export const getHeight = (data: number): number => {
  return getElObsolete(data).height
}
export const setHeight = (data: number): void => {
  getElObsolete(data).height = js.readInt(data, 4)
}

// fillStyle
export const getFillStyle = (data: number): number => {
  return js.writeString(getContextObsolete(data).fillStyle.toString())
}
export const setFillStyle = (data: number): void => {
  getContextObsolete(data).fillStyle = js.readString(data, 4)
}

// strokeStyle
export const getStrokeStyle = (data: number): number => {
  return js.writeString(getContextObsolete(data).strokeStyle.toString())
}
export const setStrokeStyle = (data: number): void => {
  getContextObsolete(data).strokeStyle = js.readString(data, 4)
}

// lineCap
export const getLineCap = (data: number): number => {
  return js.writeString(getContextObsolete(data).lineCap)
}
export const setLineCap = (data: number): void => {
  getContextObsolete(data).lineCap = js.readString(data, 4) as CanvasLineCap
}

// lineJoin
export const getLineJoin = (data: number): number => {
  return js.writeString(getContextObsolete(data).lineJoin)
}
export const setLineJoin = (data: number): void => {
  getContextObsolete(data).lineJoin = js.readString(data, 4) as CanvasLineJoin
}

// lineWidth
export const getLineWidth = (data: number): number => {
  return getContextObsolete(data).lineWidth
}
export const setLineWidth = (data: number): void => {
  getContextObsolete(data).lineWidth = js.readInt(data, 4)
}

// lineDashOffset
export const getLineDashOffset = (data: number): number => {
  return getContextObsolete(data).lineDashOffset
}
export const setLineDashOffset = (data: number): void => {
  getContextObsolete(data).lineDashOffset = js.readInt(data, 4)
}

// miterLimit
export const getMiterLimit = (data: number): number => {
  return getContextObsolete(data).miterLimit
}
export const setMiterLimit = (data: number): void => {
  getContextObsolete(data).miterLimit = js.readInt(data, 4)
}

// lineDash
export const getLineDash = (data: number): number => {
  return js.writeString(getContextObsolete(data).getLineDash().map(Math.round).join(','))
}
export const setLineDash = (data: number): void => {
  getContextObsolete(data).setLineDash(js.readString(data, 4).split(',').map(Number))
}

// font
export const getFont = (data: number): number => {
  return js.writeString(getContextObsolete(data).font.toString())
}
export const setFont = (data: number): void => {
  getContextObsolete(data).font = js.readString(data, 4)
}

// textAlign
export const getTextAlign = (data: number): number => {
  return js.writeString(getContextObsolete(data).textAlign)
}
export const setTextAlign = (data: number): void => {
  getContextObsolete(data).textAlign = js.readString(data, 4) as CanvasTextAlign
}

// textBaseline
export const getTextBaseline = (data: number): number => {
  return js.writeString(getContextObsolete(data).textBaseline)
}
export const setTextBaseline = (data: number): void => {
  getContextObsolete(data).textBaseline = js.readString(data, 4) as CanvasTextBaseline
}

/* methods */

// rects
export const clearRect = (data: number): void => {
  getContextObsolete(data).clearRect(js.readInt(data, 4), js.readInt(data, 8), js.readInt(data, 12), js.readInt(data, 16))
}
export const fillRect = (data: number): void => {
  getContextObsolete(data).fillRect(js.readInt(data, 4), js.readInt(data, 8), js.readInt(data, 12), js.readInt(data, 16))
}

// paths
export const beginPath = (data: number): void => {
  getContextObsolete(data).beginPath()
}
export const closePath = (data: number): void => {
  getContextObsolete(data).closePath()
}
export const moveTo = (data: number): void => {
  getContextObsolete(data).moveTo(js.readFloat(data, 4), js.readFloat(data, 8))
}
export const lineTo = (data: number): void => {
  getContextObsolete(data).lineTo(js.readFloat(data, 4), js.readFloat(data, 8))
}
export const arc = (data: number): void => {
  getContextObsolete(data).arc(js.readFloat(data, 4), js.readFloat(data, 8), js.readFloat(data, 12), js.readFloat(data, 16), js.readFloat(data, 20), !!js.readShort(data, 24))
}
export const stroke = (data: number): void => {
  getContextObsolete(data).stroke()
}

// areas
export const fill = (data: number): void => {
  getContextObsolete(data).fill()
}

// text
export const fillText = (data: number): void => {
  const ctx = getContextObsolete(data);
  const maxWidth = js.readInt(data, 16)
  if (maxWidth)
    ctx.fillText(js.readString(data, 4), js.readInt(data, 8), js.readInt(data, 12), maxWidth)
  else
    ctx.fillText(js.readString(data, 4), js.readInt(data, 8), js.readInt(data, 12))
}
export const strokeText = (data: number): void => {
  const ctx = getContextObsolete(data);
  const maxWidth = js.readInt(data, 16)
  if (maxWidth)
    ctx.strokeText(js.readString(data, 4), js.readInt(data, 8), js.readInt(data, 12), maxWidth)
  else
    ctx.strokeText(js.readString(data, 4), js.readInt(data, 8), js.readInt(data, 12))
}
export const measureTextWidth = (data: number): number => {
  return Math.ceil(getContextObsolete(data).measureText(js.readString(data, 4)).width)
}
export const measureTextHeight = (data: number): number => {
  const metrics = getContextObsolete(data).measureText(js.readString(data, 4));
  return Math.ceil(metrics.actualBoundingBoxAscent + metrics.actualBoundingBoxDescent);
}

// state
export const save = (id: string): void => {
  getContext(id).save();
}
export const restore = (data: number): void => {
  getContextObsolete(data).restore();
}


const contexts = new WeakMap<HTMLCanvasElement, CanvasRenderingContext2D>()

function getContext(id: string): CanvasRenderingContext2D {
  const el = objectTracker.get<HTMLCanvasElement>(id)
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

function getContextObsolete(data: number): CanvasRenderingContext2D {
  const el = getElObsolete(data)
  console.log('el:', el)
  const ctx = contexts.get(el);
  console.log('existing ctx:', ctx)
  if (ctx)
    return ctx

  const newCtx = el.getContext('2d')
  if (!newCtx)
    throw new Error('failed to create 2d context')

  newCtx.imageSmoothingEnabled = true;
  contexts.set(el, newCtx)

  console.log('new ctx:', newCtx)
  return newCtx
}

function getElObsolete(data: number): HTMLCanvasElement {
  console.log('get el id from ', data)
  const id = js.readString(data, 0)
  console.log('get object with id:', id)
  return objectTracker.get<HTMLCanvasElement>(id)
}
