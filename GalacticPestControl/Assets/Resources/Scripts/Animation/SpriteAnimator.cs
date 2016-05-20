using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using System;
using System.Linq;

[RequireComponent(typeof(SpriteRenderer))]
public class SpriteAnimator : MonoBehaviour
{
    [Header("Input texture and JSON files")]
    public Texture2D SpriteSheet;
    public TextAsset SpriteJSON;
    AsepriteAnimation asepriteAnim;

    [Header("Generated at runtime")]
    public AsepriteAnim[] SpriteAnims;
    public AsepriteAnim CurrentAnim;
    public AsepriteFrame CurrentFrame;
    public int CurrentFrameIndex;
    public bool isPlaying = true;    
    SpriteRenderer spriteRenderer;
    float animElapsed;

    void Start()
    {
        //Load the aseprite animation from JSON
        asepriteAnim = JsonUtility.FromJson<AsepriteAnimation>(SpriteJSON.text);

        //Generate animation objects from tags in aseprite json
        SpriteAnims = asepriteAnim.meta.frameTags;      
        spriteRenderer = GetComponent<SpriteRenderer>();

        //Set current animation to be first animation by default
        PlayAnim(0);        
    }

    void Update()
    {
        //Play current animation
        if (isPlaying)
        {
            animElapsed += Time.deltaTime;
            if (animElapsed >= (float)CurrentFrame.duration / 1000f)
            {
                animElapsed = 0;
                IncrementCurrentAnimFrame();
            }
        }
    }

    void IncrementCurrentAnimFrame()
    {
        //Get current frame index and increment it
        int currentFrameIndex = GetCurrentFrameIndex();
        currentFrameIndex++;
                        
        //Wrap around current animation frames
        if (currentFrameIndex > CurrentAnim.to)
        {
            currentFrameIndex = CurrentAnim.from;
        }

        //Set current frame of animation
        CurrentFrame = asepriteAnim.frames[currentFrameIndex];
        SetSpriteFromFrame(CurrentFrame);
    }

    void SetSpriteFromFrame(AsepriteFrame frame)
    {
        // TODO: Bonus points if you refactor this to cache the sprites so it isn't regenerating them, 
        //       and have them in a static dictionary so i.e. all enemies share one set of objects.
        spriteRenderer.sprite = Sprite.Create(SpriteSheet, new Rect(frame.frame.x, frame.frame.y, frame.frame.w, frame.frame.h), Vector2.zero, 32);
    }

    public int GetCurrentFrameIndex()
    {
        return asepriteAnim.frames.ToList().IndexOf(CurrentFrame);
    }


    public void PlayAnim(string animName)
    {
        //Check that the current anim isn't already playing. If it is, do nothing. This will prevent the anim from jamming when using if/else statements to set animation states.        
        AsepriteAnim desiredAnim = asepriteAnim.meta.frameTags.Where(ft => ft.name == animName).FirstOrDefault();

        if (desiredAnim != CurrentAnim)
        {
            CurrentAnim = desiredAnim;
            CurrentFrame = asepriteAnim.frames[CurrentAnim.from];
            isPlaying = true;
        }
        return;
    }

    public void PlayAnim(int animIndex)
    {
        //Check that the current anim isn't already playing. If it is, do nothing. This will prevent the anim from jamming when using if/else statements to set animation states.   
        AsepriteAnim desiredAnim = asepriteAnim.meta.frameTags[animIndex];

        if (desiredAnim != CurrentAnim)
        {
            CurrentAnim = desiredAnim;
            CurrentFrame = asepriteAnim.frames[CurrentAnim.from];
            isPlaying = true;
        }
        return;
    }

    public void PlayAnim(AsepriteAnim anim)
    {
        //Check that the current anim isn't already playing. If it is, do nothing. This will prevent the anim from jamming when using if/else statements to set animation states.   
        AsepriteAnim desiredAnim = anim;

        if (desiredAnim != CurrentAnim)
        {
            CurrentAnim = desiredAnim;
            CurrentFrame = asepriteAnim.frames[CurrentAnim.from];
            isPlaying = true;
        }
        return;
    }

}