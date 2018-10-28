import { VmDictor } from "./VmDictor";
import { VmCollocation } from "./VmCollocation";

export interface VmWord {
    Id: number;
    // HACK returned in lover case
    // name_en_Old: string;
    // HACK returned in lover case
    // name_ru_Old: string;
    // FourDaysLearnPhase: boolean;
    // fourDaysLearnPhase_Old: boolean;
    // learnDay_Old: number;
    // repeatIterationNum_Old: number;
    // nextRepeatDate_Old: Date;
    // HACK returned in lover case
    // dailyReapeatCountForEng_Old: number;
    // dailyReapeatCountForRus_Old: number;
    // dictors_en_Old?: VmDictor[];
    // dictors_ru_Old?: VmDictor[];
    // collocation_Old: VmCollocation[];

    // New

    LearnDay: Dictionary<number>;
    FourDaysLearnPhase: Dictionary<number>;
    RepeatIterationNum: Dictionary<number>;
    NextRepeatDate: Dictionary<number>;
    DailyReapeatCount: Dictionary<number>;
    LangDictionary: Dictionary<string>;
    Dictors: Dictionary<VmDictor[]>;
    Collocation: Dictionary<VmCollocation[]>;
}

////

interface Dictionary<T> {
    [Key: string]: T;
}

// export class VmLearnDay {
//     Id: number;
//     Key: string;
//     Value: string;
// }

// export class VmFourDaysLearnPhase {
//     Id: number;
//     Key: string;
//     Value: string;
// }