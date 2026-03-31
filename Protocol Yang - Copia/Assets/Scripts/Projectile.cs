using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class Projectile : MonoBehaviour
{
    public enum OrigemProjetil
    {
        Jogador,
        Inimigo
    }

    private Rigidbody2D rb;
    private Collider2D projectileCollider;
    private Transform dono;

    [SerializeField] private float velocidade = 10f;
    [SerializeField] private float tempoDeVida = 3f;
    [SerializeField] private OrigemProjetil origem = OrigemProjetil.Jogador;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        projectileCollider = GetComponent<Collider2D>();

        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody2D>();
            Debug.LogWarning("Rigidbody2D ausente no projétil, componente adicionado automaticamente.");
        }

        rb.gravityScale = 0f;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
    }

    private void Start()
    {
        if (GetComponent<Inimigo>() != null || GetComponent<Movimento>() != null)
        {
            Debug.LogWarning("Projectile.cs estava anexado em um personagem/inimigo por engano e foi desativado.");
            enabled = false;
            return;
        }

        Destroy(gameObject, tempoDeVida);
    }

    public void ConfigurarOrigem(OrigemProjetil novaOrigem, Collider2D colisorDono = null)
    {
        origem = novaOrigem;

        if (colisorDono != null)
        {
            dono = colisorDono.transform.root;

            if (projectileCollider != null)
            {
                Physics2D.IgnoreCollision(projectileCollider, colisorDono, true);
            }
        }
    }

    public void Disparar(Vector2 direcao)
    {
        if (rb == null) return;

        rb.linearVelocity = Vector2.zero;
        rb.angularVelocity = 0f;
        rb.AddForce(direcao.normalized * velocidade, ForceMode2D.Impulse);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        ProcessarImpacto(other);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        ProcessarImpacto(collision.collider);
    }

    private void ProcessarImpacto(Collider2D other)
    {
        if (other == null) return;
        if (dono != null && other.transform.root == dono) return;

        Projectile outroProjetil = other.GetComponent<Projectile>() ?? other.GetComponentInParent<Projectile>();
        if (outroProjetil != null) return;

        Inimigo inimigo = other.GetComponent<Inimigo>() ?? other.GetComponentInParent<Inimigo>();
        Movimento jogador = other.GetComponent<Movimento>() ?? other.GetComponentInParent<Movimento>();

        if (origem == OrigemProjetil.Jogador)
        {
            if (inimigo != null || other.CompareTag("Enemy"))
            {
                GameObject alvo = inimigo != null ? inimigo.gameObject : other.gameObject;
                Destroy(alvo);
                Destroy(gameObject);
                return;
            }

            if (jogador != null || other.CompareTag("Player"))
            {
                return;
            }
        }
        else
        {
            if (inimigo != null || other.CompareTag("Enemy"))
            {
                return;
            }

            if (jogador != null || other.CompareTag("Player"))
            {
                Destroy(gameObject);
                return;
            }
        }

        if (!other.isTrigger)
        {
            Destroy(gameObject);
        }
    }
}
