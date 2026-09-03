type LogLevel = 'error' | 'warn' | 'info' | 'debug' | 'trace'
type Log = (...args: unknown[]) => void
type GetLog = (topic: string) => Log

const error: GetLog = topic => (...args) => console.error(topic, '::', ...args)
const warn: GetLog = topic => (...args) => console.warn(topic, '::', ...args)
const info: GetLog = topic => (...args) => console.info(topic, '::', ...args)
const debug: GetLog = topic => (...args) => console.debug(topic, '::', ...args)
const trace: GetLog = topic => (...args) => console.debug(topic, '::', ...args)
const noop: GetLog = () => () => {
}

class Logger {
    public readonly error: Log
    public readonly warn: Log
    public readonly info: Log
    public readonly debug: Log
    public readonly trace: Log

    constructor(
        topic: string,
        error: GetLog,
        warn: GetLog,
        info: GetLog,
        debug: GetLog,
        trace: GetLog,
    ) {
        this.error = error(topic)
        this.warn = warn(topic)
        this.info = info(topic)
        this.debug = debug(topic)
        this.trace = trace(topic)
    }
}

export const getLog = ((logLevel: LogLevel): (topic: string) => Logger => {
    console.info('logLevel:', logLevel)
    switch (logLevel) {
        case 'error':
            return topic => new Logger(topic, error, noop, noop, noop, noop)
        case 'warn':
            return topic => new Logger(topic, error, warn, noop, noop, noop)
        case 'info':
            return topic => new Logger(topic, error, warn, info, noop, noop)
        case 'debug':
            return topic => new Logger(topic, error, warn, info, debug, noop)
        case 'trace':
            return topic => new Logger(topic, error, warn, info, debug, trace)
        default:
            throw new Error(`Unexpected log level: ${logLevel}`)
    }
})((localStorage.getItem('logLevel')?.toLowerCase() ?? 'info') as LogLevel)
