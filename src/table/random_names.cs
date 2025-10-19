class RandomNames
{
  public static readonly string[] Names =
  {
    "Alligator",
    "Anteater",
    "Armadillo",
    "Aurochs",
    "Axolotl",
    "Badger",
    "Bat",
    "Beaver",
    "Buffalo",
    "Camel",
    "Capybara",
    "Chameleon",
    "Cheetah",
    "Chinchilla",
    "Chipmunk",
    "Chupacabra",
    "Cormorant",
    "Coyote",
    "Crow",
    "Dingo",
    "Dinosaur",
    "Dolphin",
    "Duck",
    "Elephant",
    "Ferret",
    "Fox",
    "Frog",
    "Giraffe",
    "Gopher",
    "Grizzly",
    "Hedgehog",
    "Hippo",
    "Hyena",
    "Ibex",
    "Ifrit",
    "Iguana",
    "Jackal",
    "Jackalope",
    "Kangaroo",
    "Koala",
    "Kraken",
    "Lemur",
    "Leopard",
    "Liger",
    "Llama",
    "Manatee",
    "Mink",
    "Monkey",
    "Moose",
    "Narwhal",
    "NyanCat",
    "Orangutan",
    "Otter",
    "Panda",
    "Penguin",
    "Platypus",
    "Pumpkin",
    "Python",
    "Quagga",
    "Rabbit",
    "Raccoon",
    "Rhino",
    "Sheep",
    "Shrew",
    "Skunk",
    "SlowLoris",
    "Squirrel",
    "Tiger",
    "Turtle",
    "Walrus",
    "Wolf",
    "Wolverine",
    "Wombat",
  };
  static int currentIndexCursor = 0;
  List<int> indices = Enumerable.Range(0, Names.Length).ToList();

  public RandomNames()
  {
    shuffleIndices();
  }

  void shuffleIndices()
  {
    Random.Shared.Shuffle(indices.ToArray());
  }

  public string getNextName()
  {
    string name = Names[indices[currentIndexCursor++]];
    if (currentIndexCursor >= indices.Count)
    {
      currentIndexCursor = 0;
      shuffleIndices();
    }
    return name;
  }
}
