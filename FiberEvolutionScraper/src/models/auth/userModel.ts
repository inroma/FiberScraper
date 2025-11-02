import type BaseModelDTO from "../BaseModelDTO";

export default class UserModel implements BaseModelDTO {
  id: number;
  created: Date;
  lastUpdated: Date;
  uId: string;
  userName: string;
}