// import { VmDictor } from "./VmDictor";
import { VmCollocation } from "./VmCollocation";
import { Dictionary } from "../../../interfaces/Index";
import { VmDictor } from "./VmDictor";

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