namespace Grubs;

public partial class GadgetComponent : EntityComponent<Gadget>
{
	protected Gadget Gadget => Entity;
	protected Grub Grub => Gadget.Grub;
	protected Player Player => Grub.Player;

	public virtual void Spawn()
	{

	}

	public virtual void ClientSpawn()
	{

	}

	public virtual bool IsResolved()
	{
		return true;
	}

	public virtual void OnUse( Weapon weapon, int charge )
	{

	}

	public virtual void OnUse( Vector3 position, Rotation direction, int charge )
	{

	}

	public virtual void Touch( Entity other )
	{

	}

	public virtual void Simulate( IClient client )
	{

	}
}
