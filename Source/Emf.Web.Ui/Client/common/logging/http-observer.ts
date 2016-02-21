
import _ = require("lodash");
import moment = require("moment");
import $ = require("jquery");
import log = require("../log");

import { LogEvent, LogLevel } from "./core";
import { IObserver } from "./rx";
import { DurationWithBackoff, IDurationWithBackoff } from "../duration-with-backoff";

export class HttpObserver implements IObserver<LogEvent> {

    private static _maxEventsToBuffer = 200;
    private static _maxEventsToSendAsGroup = 20;

    private _eventBuffer: ILogEvent[] = [];
    private _timer: number;
    private _bufferTimeInMilliseconds: DurationWithBackoff;
    private _eventsIgnored = 0;

    // Endpoint at url must take a single object of type ILogEvents
    constructor(public url: string) {

        this._bufferTimeInMilliseconds = new DurationWithBackoff({
            startingDuration: moment.duration(5, "seconds"),
            backoffFactor: 2,
            maxRandomStartingOffset: moment.duration(5, "seconds"),
            maxDuration: moment.duration(120, "seconds")
        });
    }

    onNext(event: LogEvent) {

        const eventDto = this.createDto(event);

        this.bufferEvent(eventDto);

        this.scheduleSend(HttpObserver._maxEventsToSendAsGroup);
    }

    private scheduleSend(maxNumberToSend: number) {
        if (!this._timer && this._eventBuffer.length > 0) {
            this._timer = setTimeout(() => {

                const eventsToSend = _.take(this._eventBuffer, maxNumberToSend);
                this._eventBuffer = _.drop(this._eventBuffer, maxNumberToSend);

                const eventsDto = <ILogEvents>{ Events: eventsToSend };

                this.send(eventsDto)
                    .fail((xhr, status, err) => {
                        this._bufferTimeInMilliseconds.increase();
                        const newEvents = this._eventBuffer;
                        this._eventBuffer = eventsDto.Events;
                        _.forEach(newEvents, newEvent => this.bufferEvent(newEvent));
                        log.internalLogger.error(
                            "Error from {url}: {errorMessage} ({errorCode}). Trying again in {timeSpanInSeconds} seconds. {eventCount} buffered events to be sent.",
                            this.url, err, xhr.status, this._bufferTimeInMilliseconds.get().asSeconds(), this._eventBuffer.length);
                        this._timer = null;
                        this.scheduleSend(1);
                    })
                    .done(() => {
                        this._bufferTimeInMilliseconds.reset();
                        this._timer = null;
                        this.scheduleSend(HttpObserver._maxEventsToSendAsGroup);
                    });

            }, this._bufferTimeInMilliseconds.get().asMilliseconds());
        }
    }

    private bufferEvent(eventDto: ILogEvent) {
        if (this._eventBuffer.length < HttpObserver._maxEventsToBuffer) {
            this._eventsIgnored = 0;
            this._eventBuffer.push(eventDto);
        } else {
            this._eventsIgnored++;
            if (this._eventsIgnored === 1) {
                const bufferOverflowEvent = LogEvent.create(LogLevel.Warning, "Max event buffer size exceeded ({bufferSize}). Ignoring new log events.", [this._eventBuffer.length]);
                log.internalLogger.onNext(bufferOverflowEvent);
                this._eventBuffer.push(this.createDto(bufferOverflowEvent));
            }
        }
    }

    private send(eventsDto: ILogEvents): JQueryXHR {
        return $.ajax({
            url: this.url,
            type: "POST",
            contentType: "application/json",
            data: JSON.stringify(eventsDto)
        });
    }

    private createDto(event: LogEvent): ILogEvent {
        return {
            Timestamp: event.timestamp.toISOString(),
            Level: event.level.id,
            MessageTemplate: event.messageTemplate.raw,
            Properties: event.boundProperties,
            Exception: event.exception
        };
    }
}

interface ILogEvents {
    Events: ILogEvent[];
}

interface ILogEvent {
    Timestamp: string;
    Level: string;
    MessageTemplate: string;
    Properties?: any;
    Exception?: any;
}