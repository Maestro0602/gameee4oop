# Random Enemies and Boss from the Codebase

Based on the codebase analysis, here's a breakdown of 3 enemies and 1 boss found in the scripts, along with AI image generation prompts to bring them to life!

## 1. Crawler (Enemy)
**Code Context (`Crawler.cs`):** 
This is a standard crawling enemy that patrols by continuously moving across flat surfaces. The script controls movement handling components (`IsCrawling`, `Speed`) and tracks when the crawling minion pivots at edges or walls (`IsTurning`).
```csharp
public bool IsTurning { get; private set; }
public bool IsCrawling {
    get { return this.crawlRoutine != null; }
}
```
**Image Generation Prompt:**
> A 2D hand-drawn digital art style illustration of a creepy, multi-legged bug crawling along a dark, moody stone wall. Dark fantasy insectoid creature, glowing white eyes, sharp chitin plating, desolate underground cave background, cool blue and black color palette, Hollow Knight aesthetic.

## 2. Tiny Moss Fly (Enemy)
**Code Context (`TinyMossFly.cs`):** 
A small flying insect found hovering around. The code controls a `Buzz` function that utilizes a dynamic mix of `accelerationMax` and `dampener` to simulate real, erratic fly movement. It also features a unique `songMode` where it vibrates in place to a beat.
```csharp
protected void FixedUpdate() {
    float deltaTime = Time.deltaTime;
    if (!this.flyingAway && !this.songMode) {
        this.Buzz(deltaTime);
    }
    if (this.songMode) {
        // Vibrates in place when in song mode
        Vector3 vector = new Vector3(this.startX + Random.Range(-0.06f, 0.06f), this.startY + Random.Range(-0.06f, 0.06f), base.transform.position.z);
        base.transform.position = vector;
    }
}
```
**Image Generation Prompt:**
> A cute, tiny flying insect entirely covered in lush green moss. Highly detailed 2D indie game art style, glowing particles surrounding it. The creature is hovering softly in a vibrant, overgrown greenhouse setting. Magical and atmospheric lighting, soft greens and yellows.

## 3. Jelly Egg (Enemy/Hazard)
**Code Context (`JellyEgg.cs`):** 
This behaves like an environmental creature (similar to an Ooma). When prompted by a direct attack (like a "Nail Attack" or "Hero Spell"), it triggers a `Burst()`. If it's the explosive variant (`this.bomb = true`), it spawns a deadly localized explosion.
```csharp
private void OnTriggerEnter2D(Collider2D otherCollider) {
    if (otherCollider.gameObject.tag == "Nail Attack" || otherCollider.gameObject.tag == "Hero Spell" || otherCollider.gameObject.tag == "HeroBox") {
        this.Burst();
    }
}

private void Burst() {
    // ...
    if (this.bomb) {
        this.explosionObject.Spawn(base.transform.position, base.transform.localRotation);
        return;
    }
    // ...
}
```
**Image Generation Prompt:**
> A translucent, floating jellyfish-like egg sac. Inside the gelatinous membrane is a dense, glowing orange, fiery core ready to detonate. Deep sea mixed with underground cavern environment, glowing bioluminescence, 2D platformer art style, crisp outlines, mysterious and dangerous atmosphere.

## 4. Hive Knight (Boss)
**Code Context (`HiveKnightStinger.cs`):** 
This script operates the stinger projectiles shot by the Hive Knight boss. Using trigonometric math (`Mathf.Cos` and `Mathf.Sin`), it calculates a straight linear velocity along a set direction based on a high baseline `speed`. A timer is set to 2 seconds before the projectile disappears.
```csharp
private void Update() {
    float num = this.speed * Mathf.Cos(this.direction * 0.017453292f); // math to calculate trajectory X 
    float num2 = this.speed * Mathf.Sin(this.direction * 0.017453292f); // math to calculate trajectory Y
    Vector2 vector;
    vector.x = num;
    vector.y = num2;
    this.rb.linearVelocity = vector; // Apply trajectory

    if (this.timer > 0f) {
        this.timer -= Time.deltaTime;
        return;
    }
    base.gameObject.SetActive(false); // Disappears after timer expires
}
```
**Image Generation Prompt:**
> An imposing, royal anthropomorphic bee knight wearing heavily armored golden chitin. He is wielding a rapier shaped like a giant stinger, aggressively lunging forward. Honeycomb patterns in the background, glowing amber and gold lighting, dynamic action pose, 2D metroidvania boss fight style.