public struct Leveldata
{
    public int width;
    public int height;
    public string name;
    public Tiledata[,] data;
    public int cameraVerticalOffset;
    public float cameraSize;
}

public struct Tiledata
{
    public EntityID entityID;
    public Direction direction;
    public int activateNumber;
    public bool even;
    public bool odd;
    public bool less;
    public bool greater;
    public bool not;

    public Tiledata(EntityID id, int activateNumber = -1, Direction direction = Direction.none,
        bool even = false, bool odd = false, bool less = false, bool greater = false, bool not = false)
    {
        this.entityID = id;
        this.direction = direction;
        this.activateNumber = activateNumber;
        this.even = even;
        this.odd = odd;
        this.less = less;
        this.greater = greater;
        this.not = not;
    }

}