export default class MinMaxBounds {
  minX: number;
  minY: number;
  maxX: number;
  maxY: number;

  constructor(data: Array<number>) {
    this.minX = data[0];
    this.minY = data[1];
    this.maxX = data[2];
    this.maxY = data[3];
  }
}