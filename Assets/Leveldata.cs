public struct Leveldata
{
    public int width;
    public int height;
    public string name;
    public Tiledata[,] data;
}

public struct Tiledata
{
    public tileID id;
    public Direction direction;
    public int activateNumber;

    public Tiledata(tileID id, Direction direction, int activateNumber)
    {
        this.id = id;
        this.direction = direction;
        this.activateNumber = activateNumber;
    }

}