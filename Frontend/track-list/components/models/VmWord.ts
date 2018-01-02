import { VmDictor } from "./VmDictor";
import { VmName } from "./VmName";

export interface VmWord {
    Name: VmName;
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

