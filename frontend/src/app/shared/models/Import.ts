export class Import {
    processId: string;
    processedAt: Date;

    constructor(processId: string, processedAt: Date) {
        this.processId = processId;
        this.processedAt = processedAt;
    }
}