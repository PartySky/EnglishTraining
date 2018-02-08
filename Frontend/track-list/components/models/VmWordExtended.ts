import { VmWord } from "./VmWord";
import { VmName } from "./VmName";

export interface VmWordExtended extends VmWord {
    Name: VmName;
    CurrentRandomLocalization?: string;
}