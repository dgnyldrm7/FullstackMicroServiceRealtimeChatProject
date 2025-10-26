export interface ChatMessage {
  senderNumber: string;
  receiverNumber: string;
  content?: string;
  sentAt: Date;
}
