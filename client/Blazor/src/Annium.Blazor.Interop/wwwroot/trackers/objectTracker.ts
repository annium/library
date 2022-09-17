import { getLog } from '../log.js';

const objects = new Map<string, HTMLElement>()

const log = getLog('objectTracker')

const track = (id: string, el: HTMLElement): void => {
  log.trace('track', id, 'for', el)
  if (objects.has(id))
    throw new Error(`Object ${id} is already tracked`)

  el.id = id
  objects.set(id, el)
}

const get = <T extends HTMLElement>(id: string): T => {
  const object = objects.get(id)
  log.trace('get', id, 'resolved', object)
  if (!object)
    throw new Error(`Object ${id} is not tracked`)

  return object as T
}

const release = (id: string): void => {
  log.trace('release', id)
  if (!objects.delete(id))
    throw new Error(`Object ${id} is not tracked`)
}

export default {
  track,
  get,
  release
}

/* globals */
track('document.head', document.head)
track('document.body', document.body)
