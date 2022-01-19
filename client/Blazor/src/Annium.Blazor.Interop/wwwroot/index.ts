// @ts-ignore
const annium = (window.Annium = window.Annium || {})
const interop = annium.interop || (annium.interop = {})

// globals
import windowInterop from './globals/window.js'

interop.window = windowInterop

// objects
import canvas from './objects/canvas.js'
import element from './objects/element.js'

interop.canvas = canvas
interop.element = element

// trackers
import objectTracker from './trackers/objectTracker.js'

interop.objectTracker = objectTracker