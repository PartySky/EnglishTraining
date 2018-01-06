import { VmDictor } from "./VmDictor";

export interface VmWord {
    Id: number;
    // HACK returned in lover case
    name_en: string;
    // HACK returned in lover case
    name_ru: string;
    // FourDaysLearnPhase: boolean;
    FourDaysLearnPhase: boolean;
    LearnDay: number;
    RepeatIterationNum: number;
    NextRepeatDate: Date;
    // HACK returned in lover case
    dailyReapeatCountForEng: number;
    dailyReapeatCountForRus: number;
    Dictors_en?: VmDictor[];
    Dictors_ru?: any;
    // Dictors_ru?: VmDictor[];
}

