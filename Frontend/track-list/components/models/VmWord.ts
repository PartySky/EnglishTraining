// import { VmDictor } from "./VmDictor";
import { VmCollocation } from "./VmCollocation";
import { Dictionary } from "../../../interfaces/Index";

export interface VmWord {
    Id: number;
    // name_en_Old: string;
    // name_ru_Old: string;
    // fourDaysLearnPhase_Old: boolean;
    // learnDay_Old: number;
    // repeatIterationNum_Old: number;
    // nextRepeatDate_Old: Date;
    // dailyReapeatCountForEng_Old: number;
    // dailyReapeatCountForRus_Old: number;
    // dictors_en_Old?: VmDictor[];
    // dictors_ru_Old?: VmDictor[];
    // collocation_Old: VmCollocation[];

    // New

    LearnDay: Dictionary<number>;
    FourDaysLearnPhase: Dictionary<boolean>;
    RepeatIterationNum: Dictionary<number>;
    NextRepeatDate: Dictionary<Date>;
    DailyReapeatCount: Dictionary<number>;
    LangDictionary: Dictionary<string>;
    Dictors: Dictionary<VmDictor[]>;
    Collocation: Dictionary<VmCollocation[]>;
}

////


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