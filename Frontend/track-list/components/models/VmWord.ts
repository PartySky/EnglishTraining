import { VmDictor } from "./VmDictor";

export interface VmWord {
    Id: number;
    // HACK returned in lover case
    name_en: string;
    // HACK returned in lover case
    name_ru: string;
    FourDausLearnPhase: boolean;
    LearnDay: number;
    RepeatIterationNum: number;
    NextRepeatDate: string;
    DailyReapeatCountForEng: number;
    DailyReapeatCountForRus: number;
    Dictors_en?: VmDictor[];
    Dictors_ru?: any;
    // Dictors_ru?: VmDictor[];
}

