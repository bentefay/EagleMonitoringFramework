
import moment = require("moment");

export interface IDurationWithBackoff {
    startingDuration: moment.Duration;
    backoffFactor: number;
    maxRandomStartingOffset?: moment.Duration;
    maxDuration: moment.Duration;
}

export class DurationWithBackoff {

    startingDuration: moment.Duration;
    maxRandomStartingOffset: moment.Duration;
    backoffFactor: number;
    maxDuration: moment.Duration;

    private currentDuration: moment.Duration;

    constructor(config: IDurationWithBackoff) {
        this.startingDuration = config.startingDuration;
        this.maxRandomStartingOffset = config.maxRandomStartingOffset || moment.duration();
        this.backoffFactor = config.backoffFactor;
        this.maxDuration = config.maxDuration;
        this.reset();
    }

    increase() {
        const newMilliseconds = this.currentDuration.asMilliseconds() * this.backoffFactor;
        this.currentDuration = moment.duration(Math.min(newMilliseconds, this.maxDuration.asMilliseconds()));
    }

    reset() {
        this.currentDuration = moment.duration(this.startingDuration.asMilliseconds() + this.maxRandomStartingOffset.asMilliseconds() * Math.random());
    }

    get() {
        return this.currentDuration;
    }
}