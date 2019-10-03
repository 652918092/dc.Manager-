import { Pipe, PipeTransform } from '@angular/core';

import * as moment from 'moment';
import {noUndefined} from "@angular/compiler/src/util";

@Pipe({ name: 'momentFormat' })
export class MomentFormatPipe implements PipeTransform {
    transform(value: moment.MomentInput, format: string) {
        if (value) {
            return moment(value).format(format);
        } else {
            return undefined;
        }
    }
}
