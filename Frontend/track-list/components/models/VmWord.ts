import { VmDictor } from "./VmDictor";
import { VmCollocation } from "./VmCollocation";

export interface VmWord {
    Id: number;
    // HACK returned in lover case
    name_en: string;
    // HACK returned in lover case
    name_ru: string;
    // FourDaysLearnPhase: boolean;
    fourDaysLearnPhase: boolean;
    learnDay: number;
    repeatIterationNum: number;
    nextRepeatDate: Date;
    // HACK returned in lover case
    dailyReapeatCountForEng: number;
    dailyReapeatCountForRus: number;
    dictors_en?: VmDictor[];
    dictors_ru?: VmDictor[];
    collocation: VmCollocation[];
}

