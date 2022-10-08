// @ts-ignore
const annium = (window.Annium = window.Annium || {})
const interop = annium.interop || (annium.interop = {})

// globals
import * as windowInterop from './globals/window.js'

interop.window = windowInterop

// objects
import * as canvas from './objects/canvas.js'
import * as element from './objects/element.js'

interop.canvas = canvas
interop.element = element

// trackers
import cbTracker from './trackers/cbTracker.js'
import objectTracker from './trackers/objectTracker.js'

interop.cbTracker = cbTracker
interop.objectTracker = objectTracker