// noinspection JSUnusedGlobalSymbols

import objectTracker from '../trackers/objectTracker.js'

/* properties */

// width
export const getWidth = (id: string): number => {
  return getById(id).width
}
export const setWidth = (id: string, width: number): void => {
  getById(id).width = width
}

// height
export const getHeight = (id: string): number => {
  return getById(id).height
}
export const setHeight = (id: string, height: number): void => {
  getById(id).height = height
}

// fillStyle
export const getFillStyle = (id: string): string => {
  return getContext(id).fillStyle.toString()
}
export const setFillStyle = (id: string, style: string): void => {
  getContext(id).fillStyle = style
}

// strokeStyle
export const getStrokeStyle = (id: string): string => {
  return getContext(id).strokeStyle.toString()
}
export const setStrokeStyle = (id: string, style: string): void => {
  getContext(id).strokeStyle = style
}

// lineCap
export const getLineCap = (id: string): CanvasLineCap => {
  return getContext(id).lineCap
}
export const setLineCap = (id: string, lineCap: CanvasLineCap): void => {
  getContext(id).lineCap = lineCap
}

// lineJoin
export const getLineJoin = (id: string): CanvasLineJoin => {
  return getContext(id).lineJoin
}
export const setLineJoin = (id: string, lineJoin: CanvasLineJoin): void => {
  getContext(id).lineJoin = lineJoin
}

// lineWidth
export const getLineWidth = (id: string): number => {
  return getContext(id).lineWidth
}
export const setLineWidth = (id: string, width: number): void => {
  getContext(id).lineWidth = width
}

// lineDashOffset
export const getLineDashOffset = (id: string): number => {
  return getContext(id).lineDashOffset
}
export const setLineDashOffset = (id: string, offset: number): void => {
  getContext(id).lineDashOffset = offset
}

// miterLimit
export const getMiterLimit = (id: string): number => {
  return getContext(id).miterLimit
}
export const setMiterLimit = (id: string, limit: number): void => {
  getContext(id).miterLimit = limit
}

// lineDash
export const getLineDash = (id: string): string => {
  return getContext(id).getLineDash().map(Math.round).join(',')
}
export const setLineDash = (id: string, dash: string): void => {
  getContext(id).setLineDash(dash.split(',').map(Number))
}

// font
export const getFont = (id: string): string => {
  return getContext(id).font
}
export const setFont = (id: string, font: string): void => {
  getContext(id).font = font
}

// textAlign
export const getTextAlign = (id: string): CanvasTextAlign => {
  return getContext(id).textAlign
}
export const setTextAlign = (id: string, align: CanvasTextAlign): void => {
  getContext(id).textAlign = align
}

// textBaseline
export const getTextBaseline = (id: string): CanvasTextBaseline => {
  return getContext(id).textBaseline
}
export const setTextBaseline = (id: string, baseline: CanvasTextBaseline): void => {
  getContext(id).textBaseline = baseline
}

/* methods */

// rects
export const clearRect = (id: string, x: number, y: number, width: number, height: number): void => {
  getContext(id).clearRect(x, y, width, height)
}
export const fillRect = (id: string, x: number, y: number, width: number, height: number): void => {
  getContext(id).fillRect(x, y, width, height)
}

// paths
export const beginPath = (id: string): void => {
  getContext(id).beginPath()
}
export const closePath = (id: string): void => {
  getContext(id).closePath()
}
export const moveTo = (id: string, x: number, y: number): void => {
  getContext(id).moveTo(x, y)
}
export const lineTo = (id: string, x: number, y: number): void => {
  getContext(id).lineTo(x, y)
}
export const arc = (id: string, x: number, y: number, radius: number, startAngle: number, endAngle: number, antiClockwise: boolean): void => {
  getContext(id).arc(x, y, radius, startAngle, endAngle, antiClockwise)
}
export const stroke = (id: string): void => {
  getContext(id).stroke()
}

// areas
export const fill = (id: string): void => {
  getContext(id).fill()
}

// text
export const fillText = (id: string, text: string, x: number, y: number, maxWidth: number): void => {
  const ctx = getContext(id);
  if (maxWidth)
    ctx.fillText(text, x, y, maxWidth)
  else
    ctx.fillText(text, x, y)
}
export const strokeText = (id: string, text: string, x: number, y: number, maxWidth: number): void => {
  const ctx = getContext(id);
  if (maxWidth)
    ctx.strokeText(text, x, y, maxWidth)
  else
    ctx.strokeText(text, x, y)
}
export const measureTextWidth = (id: string, text: string): number => {
  return Math.ceil(getContext(id).measureText(text).width)
}
export const measureTextHeight = (id: string, text: string): number => {
  const metrics = getContext(id).measureText(text);
  return Math.ceil(metrics.actualBoundingBoxAscent + metrics.actualBoundingBoxDescent);
}

// state
export const save = (id: string): void => {
  getContext(id).save();
}
export const restore = (id: string): void => {
  getContext(id).restore();
}


const contexts = new WeakMap<HTMLCanvasElement, CanvasRenderingContext2D>()

function getContext(id: string): CanvasRenderingContext2D {
  const el = getById(id)
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

function getById(id: string): HTMLCanvasElement {
  return objectTracker.get<HTMLCanvasElement>(id)
}
