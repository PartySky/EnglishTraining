import { VmCollocation } from './VmCollocation';
import { VmDictor } from './VmDictor';
import { Dictionary } from '../interfaces/Dictionary';

export interface VmWord {
    Id: number;
    learnDay: Dictionary<number>;
    fourDaysLearnPhase: Dictionary<boolean>;
    repeatIterationNum: Dictionary<number>;
    nextRepeatDate: Dictionary<Date>;
    dailyReapeatCount: Dictionary<number>;
    langDictionary: Dictionary<string>;
    dictors: Dictionary<VmDictor[]>;
    // TODO: find out if it should be Dictionary
    // collocation: Dictionary<VmCollocation[]>;
    collocation: VmCollocation[];
}
